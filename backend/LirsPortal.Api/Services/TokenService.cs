using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LirsPortal.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace LirsPortal.Api.Services;

/// <summary>
/// Cryptographic Token Issuance Service.
/// Generates industry-standard, tamper-evident JSON Web Tokens (RFC 7519)
/// for authenticated Lagos State Internal Revenue Service portal sessions.
/// 
/// Engineering & Defense Architecture:
/// 1. Stateless Authentication: The server does not store user session tokens in memory or database.
///    Instead, the token itself contains cryptographically signed claims.
/// 2. Symmetric Cryptography: We use HMAC-SHA256 (HS256) where the server signs the token
///    using a secure key configured in appsettings / environment secrets.
/// 3. Token Anatomy:
///    - Header: Specifies the signing algorithm (HS256) and token type (JWT).
///    - Payload: Contains identity claims ('sub' for UserId, 'unique_name' for Username,
///      'role' for authorization, and 'taxpayerId' for ownership partitioning).
///    - Signature: Generated via HMACSHA256(Base64Url(Header) + "." + Base64Url(Payload), SecretKey).
///    Any tampering with payload claims (e.g. changing taxpayerId from 101 to 102) instantly
///    invalidates the signature, causing the ASP.NET Core JwtBearer middleware to reject the request.
/// </summary>
public class TokenService(IConfiguration config)
{
    public string GenerateToken(User user)
    {
        // 1. Retrieve the master cryptographic signing key from configuration or environment.
        // In production, this key must be at least 256 bits (32 characters) and stored in Azure Key Vault / AWS Secrets Manager.
        var secretKey = config["Jwt:SecretKey"] ?? "LirsSuperSecretDefaultKeyForTrainingSession2026!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 2. Define the identity claims baked into the token payload.
        // Standard JWT claim names (ClaimTypes) ensure seamless integration with ASP.NET Core's HttpContext.User.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        // 3. For citizens, attach their official TaxpayerId to enable automated Ownership Validation.
        // Revenue officers do NOT have a taxpayerId (TaxpayerId is null), granting them administrative reach.
        if (user.TaxpayerId.HasValue)
        {
            claims.Add(new Claim("taxpayerId", user.TaxpayerId.Value.ToString()));
        }

        // 4. Construct the token envelope with statutory issuer, audience, and 8-hour shift lifespan.
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "LirsPortal",
            audience: config["Jwt:Audience"] ?? "LirsPortalClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        // 5. Serialize to the canonical base64url string: <Header>.<Payload>.<Signature>
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
