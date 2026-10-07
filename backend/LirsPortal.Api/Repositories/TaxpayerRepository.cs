using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

public class TaxpayerRepository(DbConnectionFactory factory)
{
    public async Task<IEnumerable<Taxpayer>> GetPageAsync(string? tin, int page, int pageSize)
    {
        const string sql = @"
            SELECT TaxpayerId, TIN, Name, Type, State, Phone
            FROM Taxpayers
            WHERE (@Tin IS NULL OR TIN = @Tin)
            ORDER BY Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        using var db = factory.Create();
        return await db.QueryAsync<Taxpayer>(sql, new { Tin = tin, Offset = (page - 1) * pageSize, PageSize = pageSize });
    }

    public async Task<Taxpayer?> GetByIdAsync(int id)
    {
        const string sql = "SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TaxpayerId = @Id";
        using var db = factory.Create();
        return await db.QuerySingleOrDefaultAsync<Taxpayer>(sql, new { Id = id });
    }

    public async Task<IEnumerable<TaxReturn>> GetReturnsAsync(int taxpayerId)
    {
        const string sql = @"
            SELECT ReturnId, TaxpayerId, TaxYear, DeclaredIncome, TaxDue, Status
            FROM TaxReturns
            WHERE TaxpayerId = @TaxpayerId
            ORDER BY TaxYear DESC";
        using var db = factory.Create();
        return await db.QueryAsync<TaxReturn>(sql, new { TaxpayerId = taxpayerId });
    }

    public async Task<decimal> GetBalanceAsync(int taxpayerId)
    {
        const string sql = @"
            SELECT
              COALESCE((SELECT SUM(TaxDue) FROM TaxReturns
                        WHERE TaxpayerId = @TaxpayerId AND Status <> 'Draft'), 0)
              -
              COALESCE((SELECT SUM(p.Amount) FROM Payments p
                        JOIN TaxReturns r ON r.ReturnId = p.ReturnId
                        WHERE r.TaxpayerId = @TaxpayerId), 0);";
        using var db = factory.Create();
        return await db.QuerySingleAsync<decimal>(sql, new { TaxpayerId = taxpayerId });
    }
}
