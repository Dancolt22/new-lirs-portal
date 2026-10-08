using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

/// <summary>
/// Data access repository for authenticated user accounts.
/// Interacts with the 'Users' table introduced in Day 4 security migration (03_auth.sql).
/// </summary>
public class UserRepository(DbConnectionFactory factory)
{
    /// <summary>
    /// Looks up a user account by unique username.
    /// Uses parameterized SQL to prevent SQL injection during authentication attempts.
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = @"
            SELECT UserId, Username, PasswordHash, Role, TaxpayerId, FailedAttempts, LockedUntil
            FROM Users 
            WHERE Username = @Username";

        using var db = factory.Create();
        return await db.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    /// <summary>
    /// Updates the failed login attempt counter and sets the account lockout expiration.
    /// Senior Security Note:
    /// Tracking failures on the database row prevents distributed brute-force attacks across
    /// multiple browser tabs or IP proxies.
    /// </summary>
    public async Task RecordFailedAttemptAsync(int userId, int failedAttempts, DateTime? lockedUntil)
    {
        const string sql = @"
            UPDATE Users 
            SET FailedAttempts = @FailedAttempts, LockedUntil = @LockedUntil 
            WHERE UserId = @UserId";

        using var db = factory.Create();
        await db.ExecuteAsync(sql, new { UserId = userId, FailedAttempts = failedAttempts, LockedUntil = lockedUntil });
    }

    /// <summary>
    /// Resets the failed attempts counter to 0 and clears any active lockouts
    /// upon a successful, verified password challenge.
    /// </summary>
    public async Task ResetFailedAttemptsAsync(int userId)
    {
        const string sql = @"
            UPDATE Users 
            SET FailedAttempts = 0, LockedUntil = NULL 
            WHERE UserId = @UserId";

        using var db = factory.Create();
        await db.ExecuteAsync(sql, new { UserId = userId });
    }
}
