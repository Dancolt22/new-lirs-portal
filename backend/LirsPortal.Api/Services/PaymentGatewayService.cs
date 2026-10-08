namespace LirsPortal.Api.Services;

/// <summary>
/// Abstraction contract representing an external financial settlement gateway
/// (e.g., Interswitch, Remita, Paystack, Flutterwave, or NIBSS).
/// 
/// Engineering & Architecture Defense:
/// Decoupling payment settlement behind an interface allows:
/// 1. Zero-dependency unit testing with fast in-memory fakes.
/// 2. Switching banking gateways or implementing multi-gateway fallback without altering business logic.
/// 3. Standardizing timeout handling, retry policies, and circuit breakers.
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>
    /// Confirms with the banking network that funds have truly cleared and settled
    /// before LIRS issues a legally binding government tax clearance receipt.
    /// </summary>
    /// <param name="reference">Unique banking transaction reference identifier</param>
    /// <param name="expectedAmount">The exact currency amount expected to have settled</param>
    /// <param name="cancellationToken">Allows canceling if upstream client disconnects</param>
    /// <returns>True if settled; false if transaction failed or declined</returns>
    Task<bool> VerifyTransactionAsync(string reference, decimal expectedAmount, CancellationToken cancellationToken = default);
}

/// <summary>
/// High-fidelity mock implementation of an external payment settlement gateway.
/// Simulates real-world network latency, settlement validation, and edge-case failure testing.
/// </summary>
public class MockPaymentGatewayService(ILogger<MockPaymentGatewayService> logger) : IPaymentGatewayService
{
    public async Task<bool> VerifyTransactionAsync(string reference, decimal expectedAmount, CancellationToken cancellationToken = default)
    {
        // Resiliency Pattern: Strict 10-second timeout boundary.
        // If an external banking gateway hangs, we abort the request rather than exhausting API server worker threads.
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        // Sanitized Structured Logging: In banking integration, never log PANs, CVVs, or cardholder secrets.
        // We log the transaction reference and authorized settlement amount for auditing.
        logger.LogInformation("Verifying reference {Ref} with external gateway for amount {Amount:N2}", reference, expectedAmount);

        // Simulate network latency of talking to an external banking host (100ms)
        await Task.Delay(100, cts.Token);

        // Deterministic Test Harness:
        // Any test transaction reference starting with "FAIL-" will simulate a declined or insufficient funds settlement.
        if (reference.StartsWith("FAIL-", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("External gateway verification declined reference {Ref}", reference);
            return false;
        }

        logger.LogInformation("External gateway confirmed settlement for reference {Ref}", reference);
        return true;
    }
}
