using LirsPortal.Api.Services;
using Xunit;

namespace LirsPortal.Tests;

/// <summary>
/// Unit test suite verifying late penalty calculations for the Lagos State Internal Revenue Service.
/// 
/// Engineering & Defense Principles:
/// 1. Pattern: Follows the standard Arrange-Act-Assert (AAA) methodology.
/// 2. [Fact] vs [Theory]:
///    - [Fact] is a test that tests a single deterministic condition (e.g., zero penalty when on time).
///    - [Theory] is a parameterized test that runs the exact same assertion across multiple datasets
///      supplied via [InlineData], proving the algorithm across different business magnitudes.
/// 3. Boundary & Edge Testing: Validates not just normal positive numbers, but edge conditions:
///    zero days late, negative days, waived penalties, and negative amounts.
/// </summary>
public class PenaltyCalculatorTests
{
    private readonly PenaltyCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenOnTime_ReturnsZero()
    {
        // Arrange: Taxpayer paid exactly on the statutory due date (0 days late)
        decimal amountDue = 100000m;
        int daysLate = 0;

        // Act: Invoke the calculation engine
        decimal penalty = _calculator.Calculate(amountDue, daysLate);

        // Assert: Ensure statutory zero penalty is assessed
        Assert.Equal(0m, penalty);
    }

    [Fact]
    public void Calculate_WhenWaived_ReturnsZeroEvenIfLate()
    {
        // Arrange: Executive waiver granted under special state dispensation
        decimal amountDue = 100000m;
        int daysLate = 15;
        bool isWaived = true;

        // Act
        decimal penalty = _calculator.Calculate(amountDue, daysLate, isWaived);

        // Assert
        Assert.Equal(0m, penalty);
    }

    [Theory]
    [InlineData(100000, 5, 10500)]   // 10% (10,000) + 5 days * ₦100 (500) = ₦10,500
    [InlineData(50000, 1, 5100)]     // 10% (5,000) + 1 day * ₦100 (100) = ₦5,100
    [InlineData(240000, 10, 25000)]  // 10% (24,000) + 10 days * ₦100 (1,000) = ₦25,000
    public void Calculate_WhenLate_ReturnsCorrectTenPercentPlusDailyNaira(decimal due, int days, decimal expected)
    {
        // Act: Execute calculation for each parameterized dataset
        decimal penalty = _calculator.Calculate(due, days);

        // Assert: Verify exact arithmetic match
        Assert.Equal(expected, penalty);
    }

    [Fact]
    public void Calculate_WhenAmountDueIsNegative_ReturnsZero()
    {
        // Arrange: Negative balance (credit carryforward) must never incur a penalty
        decimal amountDue = -50000m;
        int daysLate = 3;

        // Act
        decimal penalty = _calculator.Calculate(amountDue, daysLate);

        // Assert
        Assert.Equal(0m, penalty);
    }

    [Fact]
    public void Calculate_WhenDaysLateIsNegative_ReturnsZero()
    {
        // Arrange: Early filing (days late < 0) must never incur a penalty
        decimal amountDue = 100000m;
        int daysLate = -5;

        // Act
        decimal penalty = _calculator.Calculate(amountDue, daysLate);

        // Assert
        Assert.Equal(0m, penalty);
    }
}
