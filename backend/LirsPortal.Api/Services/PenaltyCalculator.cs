namespace LirsPortal.Api.Services;

/// <summary>
/// Encapsulates statutory tax late-payment penalty calculations for Lagos State.
/// Rule: 10% statutory penalty on tax due + ₦100 daily fee per day late.
/// Returns ₦0.00 if waived by state tax board, or if filed on time.
/// </summary>
public class PenaltyCalculator
{
    private const decimal PercentageRate = 0.10m; // 10% statutory penalty
    private const decimal DailyFee = 100.00m;      // ₦100 statutory per-day late fee

    /// <summary>
    /// Computes total penalty based on amount due and number of overdue days.
    /// </summary>
    public decimal Calculate(decimal amountDue, int daysLate, bool isWaived = false)
    {
        // Guard clauses: If waived, filed on-time, or invalid due amount, penalty is zero
        if (isWaived || daysLate <= 0 || amountDue <= 0)
        {
            return 0m;
        }

        decimal percentagePenalty = amountDue * PercentageRate;
        decimal dailyPenalty = daysLate * DailyFee;

        return percentagePenalty + dailyPenalty;
    }
}
