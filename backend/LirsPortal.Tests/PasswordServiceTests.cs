using LirsPortal.Api.Services;
using Xunit;

namespace LirsPortal.Tests;

/// <summary>
/// Unit tests verifying PBKDF2 password hashing and constant-time verification.
/// </summary>
public class PasswordServiceTests
{
    private readonly PasswordService _passwords = new();

    [Fact]
    public void HashPassword_ProducesValidSaltedFormat()
    {
        string hash = _passwords.HashPassword("pass123");
        
        // Format: {iterations}.{base64-salt}:{base64-subKey}
        Assert.Contains("10000.", hash);
        Assert.Contains(":", hash);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        string hash = _passwords.HashPassword("pass123");
        bool isValid = _passwords.VerifyPassword("pass123", hash);
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ReturnsFalse()
    {
        string hash = _passwords.HashPassword("pass123");
        bool isValid = _passwords.VerifyPassword("wrongpass", hash);
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_WithMalformedHash_ReturnsFalseWithoutThrowing()
    {
        bool isValid = _passwords.VerifyPassword("pass123", "corrupted.hash:invalid");
        Assert.False(isValid);
    }
}
