using System.ComponentModel.DataAnnotations;

namespace LirsPortal.Api.Models;

/// <summary>
/// Domain model representing a registered taxpayer in Lagos State.
/// Maps directly to rows in the SQL Server 'Taxpayers' table.
/// </summary>
public record Taxpayer(
    int TaxpayerId, 
    string TIN, 
    string Name, 
    string Type, 
    string State, 
    string Phone);

/// <summary>
/// Domain model representing an annual tax assessment filed by a taxpayer.
/// Maps directly to rows in the SQL Server 'TaxReturns' table.
/// </summary>
public record TaxReturn(
    int ReturnId, 
    int TaxpayerId, 
    int TaxYear, 
    decimal DeclaredIncome, 
    decimal TaxDue, 
    string Status);

/// <summary>
/// Domain model representing a payment transaction record.
/// Maps directly to rows in the SQL Server 'Payments' table.
/// </summary>
public record Payment(
    int PaymentId, 
    int ReturnId, 
    decimal Amount, 
    DateTime PaidOn, 
    string Channel);

/// <summary>
/// Audit trail entity for regulatory compliance (NDPR).
/// Tracks actions performed by officers or system events.
/// </summary>
public record ComplianceLog(
    int LogId, 
    int TaxpayerId, 
    string Event, 
    string OfficerName, 
    DateTime LoggedAt);

/// <summary>
/// User identity entity representing an authenticated portal account.
/// Maps directly to the 'Users' table created in 03_auth.sql.
/// Notice PasswordHash stores salted PBKDF2 hashes—never plain text.
/// FailedAttempts and LockedUntil protect against brute-force attacks.
/// </summary>
public record User(
    int UserId, 
    string Username, 
    string PasswordHash, 
    string Role, 
    int? TaxpayerId, 
    int FailedAttempts, 
    DateTime? LockedUntil);

/// <summary>
/// DTO capturing credentials submitted by a user on the sign-in form.
/// </summary>
public record LoginRequest(
    [Required(ErrorMessage = "Username is required.")] string Username = "", 
    [Required(ErrorMessage = "Password is required.")] string Password = "")
{
    public LoginRequest() : this("", "") {}
}

/// <summary>
/// DTO returned upon successful authentication.
/// Contains the signed JWT token, username, role ('Taxpayer' or 'Officer'),
/// and the associated TaxpayerId (which will be null for revenue officers).
/// </summary>
public record LoginResponse(
    string Token, 
    string Username, 
    string Role, 
    int? TaxpayerId);

/// <summary>
/// Alias for LoginResponse matching curriculum naming conventions.
/// </summary>
public record LoginResult(
    string Token, 
    string Username, 
    string Role, 
    int? TaxpayerId) : LoginResponse(Token, Username, Role, TaxpayerId);

/// <summary>
/// DTO capturing client input when recording a tax payment.
/// Now includes an optional transaction 'Reference' from commercial banks
/// or payment gateways to guarantee idempotency and settlement verification.
/// </summary>
public record PaymentRequest(
    [Required(ErrorMessage = "ReturnId is required.")] int ReturnId = 0,
    [Range(0.01, 1000000000.00, ErrorMessage = "Amount must be between ₦0.01 and ₦1,000,000,000.00")] decimal Amount = 0,
    [RegularExpression("^(Bank|Card|USSD)$", ErrorMessage = "Channel must be Bank, Card, or USSD")] string Channel = "Bank",
    string? Reference = null)
{
    public PaymentRequest() : this(0, 0, "Bank", null) {}
};

/// <summary>
/// Structured receipt response returned with HTTP 201 Created.
/// </summary>
public record PaymentResult(int PaymentId, string ReceiptNumber, string Status);

/// <summary>
/// Outstanding balance response payload.
/// </summary>
public record BalanceResult(int TaxpayerId, decimal Balance);

/// <summary>
/// Standardized JSON error response returned on 400, 401, 403, 404, or 500 status codes.
/// </summary>
public record ApiError(string Error, string Code);

/// <summary>
/// External payment gateway verification request contract.
/// </summary>
public record GatewayVerifyRequest(string Reference, decimal Amount);

/// <summary>
/// External payment gateway verification response contract.
/// </summary>
public record GatewayVerifyResponse(bool IsValid, string Status, string Message);
