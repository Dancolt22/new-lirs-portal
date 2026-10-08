using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

/// <summary>
/// Abstraction for payment data operations.
/// Senior Engineer Note:
/// Decoupling the repository behind an interface enables true unit testing.
/// Our unit test suite can inject an in-memory FakePaymentRepository, allowing tests
/// to execute in milliseconds without requiring a running SQL Server instance.
/// </summary>
public interface IPaymentRepository
{
    Task<TaxReturn?> GetReturnAsync(int returnId);
    Task<decimal> GetTotalPaidAsync(int returnId);
    Task<int> SaveAsync(PaymentRequest request);
    Task<bool> ExistsByReferenceAsync(string reference);
}
