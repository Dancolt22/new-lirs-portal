using System.ComponentModel.DataAnnotations;

namespace LirsPortal.Api.Models;

public class Taxpayer
{
    public int TaxpayerId { get; set; }
    public string TIN { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string State { get; set; } = "";
    public string? Phone { get; set; }
}

public class TaxReturn
{
    public int ReturnId { get; set; }
    public int TaxpayerId { get; set; }
    public int TaxYear { get; set; }
    public decimal DeclaredIncome { get; set; }
    public decimal TaxDue { get; set; }
    public string Status { get; set; } = "";
}

public class PaymentRequest
{
    [Required]
    public int ReturnId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000000", ErrorMessage = "Amount must be above zero.")]
    public decimal Amount { get; set; }

    [Required, RegularExpression("^(Bank|Card|USSD)$", ErrorMessage = "Channel must be Bank, Card, or USSD.")]
    public string Channel { get; set; } = "";
}

public record PaymentResult(int PaymentId, string ReceiptNumber, string Status);
public record BalanceResult(int TaxpayerId, decimal Balance);
public record ApiError(string Error, string Code);
