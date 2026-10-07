using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

public class PaymentRepository(DbConnectionFactory factory)
{
    public async Task<TaxReturn?> GetReturnAsync(int returnId)
    {
        const string sql = @"SELECT ReturnId, TaxpayerId, TaxYear, DeclaredIncome, TaxDue, Status
                             FROM TaxReturns WHERE ReturnId = @ReturnId";
        using var db = factory.Create();
        return await db.QuerySingleOrDefaultAsync<TaxReturn>(sql, new { ReturnId = returnId });
    }

    public async Task<decimal> GetTotalPaidAsync(int returnId)
    {
        const string sql = "SELECT COALESCE(SUM(Amount), 0) FROM Payments WHERE ReturnId = @ReturnId";
        using var db = factory.Create();
        return await db.QuerySingleAsync<decimal>(sql, new { ReturnId = returnId });
    }

    public async Task<int> SaveAsync(PaymentRequest request)
    {
        const string sql = @"
            INSERT INTO Payments (ReturnId, Amount, PaidOn, Channel)
            OUTPUT INSERTED.PaymentId
            VALUES (@ReturnId, @Amount, SYSDATETIME(), @Channel);";
        using var db = factory.Create();
        return await db.ExecuteScalarAsync<int>(sql, request);
    }
}
