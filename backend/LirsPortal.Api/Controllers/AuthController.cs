using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using LirsPortal.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

/// <summary>
/// Authentication and Access Token Issuance Controller.
/// Provides secure credential validation, brute-force lockout protection, and JWT generation.
/// 
/// Security Defense Patterns Implemented:
/// 1. OWASP A07: Identification and Authentication Failures Defense.
///    - Password verification uses slow PBKDF2 hashing with salt, immune to lookup table attacks.
/// 2. Brute-Force Rate Limiting & Account Lockout:
///    - Tracks failed attempts in SQL Server. After 5 consecutive invalid passwords, the account
///      is locked for 15 minutes (HTTP 423 Locked).
/// 3. Information Disclosure Prevention (Safe Error Masking):
///    - Returns the exact same generic error message ("Invalid username or password.") whether
///      the username does not exist or the password was incorrect, preventing username enumeration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserRepository users, 
    PasswordService passwords, 
    TokenService tokens, 
    ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Authenticates citizen or officer credentials and issues a signed JSON Web Token.
    /// </summary>
    /// <param name="request">Username and plaintext password payload</param>
    /// <returns>HTTP 200 with JWT token and profile info, or HTTP 401/423 on failure</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Retrieve the user record from SQL Server by username.
        var user = await users.GetByUsernameAsync(request.Username);

        // 2. Lockout Gatekeeper: Check if this account is currently undergoing a lockout penalty.
        if (user is not null && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
        {
            logger.LogWarning("Login rejected: account {Username} is currently locked until {LockedUntil}", 
                request.Username, user.LockedUntil.Value);
            return StatusCode(423, new ApiError("Account is temporarily locked. Please try again later.", "ACCOUNT_LOCKED"));
        }

        // 3. Credential Verification:
        // Use constant-time PBKDF2 verification via PasswordService.
        if (user is null || !passwords.VerifyPassword(request.Password, user.PasswordHash))
        {
            // If the user actually exists in the database, increment their failed attempt counter.
            if (user is not null)
            {
                int newAttempts = user.FailedAttempts + 1;
                // If they hit 5 consecutive failed attempts, enforce a 15-minute lockout penalty.
                DateTime? lockUntil = newAttempts >= 5 ? DateTime.UtcNow.AddMinutes(15) : null;
                await users.RecordFailedAttemptAsync(user.UserId, newAttempts, lockUntil);

                if (lockUntil.HasValue)
                {
                    logger.LogWarning("Account {Username} locked after {Attempts} failed attempts", user.Username, newAttempts);
                }
            }

            // Safe Error Masking: NEVER return "Username does not exist" vs "Incorrect password".
            // Doing so allows hackers to enumerate valid tax officer and citizen usernames.
            return Unauthorized(new ApiError("Invalid username or password.", "UNAUTHORIZED"));
        }

        // 4. Credential Success! Reset failed attempts counter back to zero if needed.
        if (user.FailedAttempts > 0)
        {
            await users.ResetFailedAttemptsAsync(user.UserId);
        }

        // 5. Generate cryptographically signed JSON Web Token.
        var tokenString = tokens.GenerateToken(user);
        logger.LogInformation("User {Username} ({Role}) logged in successfully", user.Username, user.Role);

        // 6. Return token and user profile metadata to the client.
        return Ok(new LoginResult(tokenString, user.Username, user.Role, user.TaxpayerId));
    }
}
