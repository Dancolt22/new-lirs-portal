using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;

namespace LirsPortal.Tests;

/// <summary>
/// In-Memory Test Double (Fake) implementing IPaymentRepository.
/// 
/// Engineering & Defense Concept (Test Doubles):
/// A "Fake" is a working implementation of an interface that takes shortcuts, making it
/// unsuitable for production (no disk persistence) but ideal for automated unit testing.
/// 
/// Why not test directly against SQL Server for unit tests?
/// 1. Speed: In-memory collections execute in fractions of a millisecond; database queries take 10-50ms.
///    A suite of 500 unit tests runs in under 1 second instead of 25 seconds.
/// 2. Isolation & Determinism: Tests do not leave dirty state in SQL Server or require a live database.
/// 3. Simplicity: We can precisely simulate edge cases (e.g., negative balances, draft returns)
///    simply by configuring the in-memory collections before invoking the method under test.
/// </summary>
public class FakePaymentRepository : IPaymentRepository
{
    public List<TaxReturn> Returns { get; set; } = [];
    public List<PaymentRequest> SavedPayments { get; set; } = [];
    public decimal TotalPaidToReturn { get; set; } = 0m;
    public HashSet<string> ExistingReferences { get; set; } = [];

    public Task<TaxReturn?> GetReturnAsync(int returnId)
    {
        var match = Returns.FirstOrDefault(r => r.ReturnId == returnId);
        return Task.FromResult(match);
    }

    public Task<decimal> GetTotalPaidAsync(int returnId)
    {
        return Task.FromResult(TotalPaidToReturn);
    }

    public Task<int> SaveAsync(PaymentRequest request)
    {
        SavedPayments.Add(request);
        return Task.FromResult(SavedPayments.Count);
    }

    public Task<bool> ExistsByReferenceAsync(string reference)
    {
        return Task.FromResult(ExistingReferences.Contains(reference));
    }
}
