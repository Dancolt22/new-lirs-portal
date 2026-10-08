using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

/// <summary>
/// Handles all SQL Server database operations related to Taxpayers and Tax Returns.
/// 
/// Senior Engineer Note for Students:
/// Notice how this class contains ZERO HTTP or web logic. It doesn't know about JSON,
/// status codes, or browser requests. Its sole job is to talk to SQL Server via Dapper,
/// execute parameterized queries, and return strongly-typed C# objects back to whoever asked.
/// </summary>
public class TaxpayerRepository(DbConnectionFactory factory)
{
    /// <summary>
    /// Fetches a paginated slice of taxpayers, with an optional filter by 10-digit TIN.
    /// </summary>
    public async Task<IEnumerable<Taxpayer>> GetPageAsync(string? tin, int page, int pageSize)
    {
        // Senior Tip: We use OFFSET / FETCH NEXT instead of loading the entire table into memory.
        // If LIRS has 2 million taxpayers, loading all rows would crash the server with OutOfMemoryException.
        // This query only asks the database engine for exactly the 20 rows needed for the active screen.
        const string sql = @"
            SELECT TaxpayerId, TIN, Name, Type, State, Phone
            FROM Taxpayers
            WHERE (@Tin IS NULL OR TIN = @Tin)
            ORDER BY Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        // Open a lightweight database connection via our factory.
        // The 'using' keyword ensures the connection is automatically closed and returned
        // to the connection pool the microsecond this method finishes, preventing connection leaks.
        using var db = factory.Create();

        // Parameterization Alert: Notice the anonymous object { Tin = tin, ... }.
        // Dapper automatically binds these as true SQL parameters.
        // Even if an attacker enters "1000000001' OR '1'='1" into the search box,
        // SQL Server treats it purely as harmless string text, completely neutralizing SQL Injection!
        return await db.QueryAsync<Taxpayer>(sql, new 
        { 
            Tin = tin, 
            Offset = (page - 1) * pageSize, 
            PageSize = pageSize 
        });
    }

    /// <summary>
    /// Finds a single taxpayer by their unique primary key ID.
    /// </summary>
    public async Task<Taxpayer?> GetByIdAsync(int id)
    {
        const string sql = "SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TaxpayerId = @Id";
        
        using var db = factory.Create();

        // QuerySingleOrDefaultAsync expects 0 or 1 record.
        // If ID 101 exists, it maps to a Taxpayer object. If ID 999 doesn't exist, it returns null safely.
        return await db.QuerySingleOrDefaultAsync<Taxpayer>(sql, new { Id = id });
    }

    /// <summary>
    /// Retrieves all filed tax returns for a specific taxpayer, ordered newest first.
    /// </summary>
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

    /// <summary>
    /// Calculates the exact outstanding tax liability balance for a taxpayer.
    /// Formula: Total Statutory Tax Due (excluding Drafts) MINUS Total Payments made to date.
    /// </summary>
    public async Task<decimal> GetBalanceAsync(int taxpayerId)
    {
        // Out-of-Reach SQL Concept Explained:
        // Why COALESCE?
        // If a taxpayer has just registered and made zero payments, SQL's SUM(Amount) returns NULL,
        // not 0. In SQL math, "500000 - NULL" equals NULL!
        // COALESCE(..., 0) converts any NULL into a clean 0.00 so subtraction always succeeds.
        //
        // Why Status <> 'Draft'?
        // Draft returns are unfiled citizen drafts. Citizens do not legally owe tax on a draft!
        const string sql = @"
            SELECT
              COALESCE((SELECT SUM(TaxDue) FROM TaxReturns
                        WHERE TaxpayerId = @TaxpayerId AND Status <> 'Draft'), 0)
              -
              COALESCE((SELECT SUM(p.Amount) FROM Payments p
                        JOIN TaxReturns r ON r.ReturnId = p.ReturnId
                        WHERE r.TaxpayerId = @TaxpayerId), 0);";

        using var db = factory.Create();
        
        // QuerySingleAsync returns the single scalar decimal balance
        return await db.QuerySingleAsync<decimal>(sql, new { TaxpayerId = taxpayerId });
    }
}
