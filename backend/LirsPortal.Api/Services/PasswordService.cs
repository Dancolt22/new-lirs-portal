using System.Security.Cryptography;

namespace LirsPortal.Api.Services;

/// <summary>
/// Provides NIST-compliant password hashing and verification services.
/// Uses PBKDF2 (Password-Based Key Derivation Function 2) with HMAC-SHA256.
/// </summary>
public class PasswordService
{
    private const int SaltSize = 16;      // 128-bit cryptographically secure random salt
    private const int KeySize = 32;       // 256-bit derived subkey
    private const int Iterations = 10000; // PBKDF2 iteration count to defeat GPU cracking

    /// <summary>
    /// Generates a salted, hashed representation of a plain-text password.
    /// Senior Cryptography Note:
    /// Format: {iterations}.{base64-salt}:{base64-subKey}
    /// A random 16-byte salt is generated for EVERY password, ensuring two identical passwords
    /// result in completely different hashes, neutralizing precomputed rainbow tables.
    /// </summary>
    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] subKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}:{Convert.ToBase64String(subKey)}";
    }

    /// <summary>
    /// Verifies a candidate plain-text password against a stored hash string.
    /// Senior Security Note on 'CryptographicOperations.FixedTimeEquals':
    /// Standard string equality (==) returns false the instant the first differing character is found.
    /// An attacker can measure nanosecond timing differences to guess hash characters byte-by-byte.
    /// FixedTimeEquals takes the exact same amount of time regardless of how many bytes match,
    /// completely defeating timing attacks.
    /// </summary>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split(['.', ':'], 3);
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out int iterations)) return false;
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expectedSubKey = Convert.FromBase64String(parts[2]);

        byte[] actualSubKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedSubKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualSubKey, expectedSubKey);
    }
}
