using LirsPortal.Api;
using LirsPortal.Api.Models;
using LirsPortal.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LirsPortal.Tests;

/// <summary>
/// Unit test suite for PaymentService.
/// Validates financial business rules, idempotency, and gateway settlement logic in complete isolation.
/// 
/// Engineering & Defense Concept (Isolation & Mocking):
/// Notice that PaymentService is tested without touching SQL Server, Kestrel, or external banking hosts.
/// We supply:
/// 1. FakePaymentRepository: Fast in-memory state store.
/// 2. MockPaymentGatewayService: Simulates settlement responses.
/// 3. NullLogger: Silent logger from Microsoft.Extensions.Logging.Abstractions that avoids console spam.
/// </summary>
public class PaymentServiceTests
{
    private readonly FakePaymentRepository _repo = new();
    private readonly IPaymentGatewayService _gateway = new MockPaymentGatewayService(NullLogger<MockPaymentGatewayService>.Instance);
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        // Assemble the system under test with its in-memory test doubles
        _service = new PaymentService(_repo, _gateway, NullLogger<PaymentService>.Instance);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenReturnDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange: Repository is completely empty; ReturnId 999 does not exist
        var request = new PaymentRequest { ReturnId = 999, Amount = 10000m, Channel = "Bank" };

        // Act & Assert: Verify that NotFoundException is thrown with descriptive error
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.RecordPaymentAsync(request));
        Assert.Equal("Return not found.", ex.Message);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenReturnIsDraft_ThrowsBusinessRuleException()
    {
        // Arrange: A tax return exists but is still in unfiled 'Draft' status
        _repo.Returns.Add(new TaxReturn(1, 101, 2025, 5000000m, 500000m, "Draft"));
        var request = new PaymentRequest { ReturnId = 1, Amount = 50000m, Channel = "Bank" };

        // Act & Assert: Unfiled assessments cannot receive payments
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RecordPaymentAsync(request));
        Assert.Equal("Payments cannot be recorded against a draft return.", ex.Message);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenPaymentExceedsOutstanding_ThrowsBusinessRuleException()
    {
        // Arrange: Tax Due is ₦500,000; citizen already paid ₦300,000 -> Outstanding is ₦200,000
        _repo.Returns.Add(new TaxReturn(1, 101, 2025, 5000000m, 500000m, "Submitted"));
        _repo.TotalPaidToReturn = 300000m;

        // Citizen attempts to pay ₦250,000 (which exceeds ₦200,000 outstanding)
        var request = new PaymentRequest { ReturnId = 1, Amount = 250000m, Channel = "Card" };

        // Act & Assert: Anti-overpayment rule blocks the transaction
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RecordPaymentAsync(request));
        Assert.Contains("exceeds the outstanding", ex.Message);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenValid_ReturnsReceiptWithExpectedFormat()
    {
        // Arrange: Outstanding is ₦500,000; valid payment of ₦150,000
        _repo.Returns.Add(new TaxReturn(1, 101, 2025, 5000000m, 500000m, "Approved"));
        _repo.TotalPaidToReturn = 0m;

        var request = new PaymentRequest { ReturnId = 1, Amount = 150000m, Channel = "Bank" };

        // Act
        var result = await _service.RecordPaymentAsync(request);

        // Assert: Receipt generated with official zero-padded statutory format (RCT-000001)
        Assert.Equal("Successful", result.Status);
        Assert.Equal("RCT-000001", result.ReceiptNumber);
        Assert.Single(_repo.SavedPayments);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenDuplicateReference_ThrowsBusinessRuleException()
    {
        // Arrange: The bank reference 'REF-TXN-12345' was already recorded previously
        _repo.Returns.Add(new TaxReturn(1, 101, 2025, 5000000m, 500000m, "Approved"));
        _repo.ExistingReferences.Add("REF-TXN-12345");

        var duplicateRequest = new PaymentRequest { ReturnId = 1, Amount = 50000m, Channel = "Bank", Reference = "REF-TXN-12345" };

        // Act & Assert: Idempotency guard prevents duplicate transaction entry
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RecordPaymentAsync(duplicateRequest));
        Assert.Contains("has already been processed", ex.Message);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenGatewayDeclines_ThrowsBusinessRuleException()
    {
        // Arrange: Valid return, but gateway returns settlement failure for 'FAIL-REF-99'
        _repo.Returns.Add(new TaxReturn(1, 101, 2025, 5000000m, 500000m, "Approved"));

        var declinedRequest = new PaymentRequest { ReturnId = 1, Amount = 50000m, Channel = "Card", Reference = "FAIL-REF-99" };

        // Act & Assert: Rejected by payment gateway settlement check
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RecordPaymentAsync(declinedRequest));
        Assert.Contains("Payment gateway could not confirm settlement", ex.Message);
    }
}
