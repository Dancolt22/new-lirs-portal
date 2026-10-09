using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;

namespace LirsPortal.Api.Services;

/// <summary>
/// Contains the core state government business logic and financial rule enforcement for LIRS payments.
/// 
/// Core Engineering Architecture Principles:
/// 1. "Controllers handle HTTP; Services handle Rules; Repositories handle SQL."
/// 2. Testability & Dependency Inversion: PaymentService depends entirely on abstractions
///    (IPaymentRepository and IPaymentGatewayService), allowing tests to execute in milliseconds
///    using in-memory fakes without hitting SQL Server or real banking networks.
/// 3. Idempotency & Gateway Verification: Guarantees that duplicate transaction clicks or network
///    retries never record duplicate credits against a citizen's assessment.
/// </summary>
public class PaymentService(
    IPaymentRepository payments, 
    IPaymentGatewayService gateway, 
    ILogger<PaymentService> logger)
{
    /// <summary>
    /// Enforces statutory rules before recording any citizen payment against a tax return.
    /// </summary>
    /// <param name="request">The incoming payment payload (ReturnId, Amount, Channel, Reference)</param>
    /// <param name="cancellationToken">Cancellation token for upstream client disconnection</param>
    /// <returns>A PaymentResult containing the assigned ID and official LIRS receipt number</returns>
    /// <exception cref="NotFoundException">Thrown if the specified tax return does not exist</exception>
    /// <exception cref="BusinessRuleException">Thrown if payment violates business rules (e.g. overpayment, draft return, duplicate reference, failed settlement)</exception>
    public async Task<PaymentResult> RecordPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        // Rule 1: Existence Check. Does this return actually exist in the state database?
        var taxReturn = await payments.GetReturnAsync(request.ReturnId)
            ?? throw new NotFoundException("Return not found.");

        // Rule 2: State Lifecycle Check. Is the return ready to receive payments?
        // Citizens cannot pay against unfiled "Draft" forms.
        if (taxReturn.Status == "Draft")
            throw new BusinessRuleException("Payments cannot be recorded against a draft return.");

        // Rule 3: Liability Arithmetic Check. How much is still legally outstanding on this return?
        // Outstanding = Total Statutory Tax Due - Amount Already Paid in Previous Transactions
        var alreadyPaid = await payments.GetTotalPaidAsync(request.ReturnId);
        var outstanding = taxReturn.TaxDue - alreadyPaid;

        // If the return has already been fully settled or in credit, prevent negative/confusing error displays
        if (outstanding <= 0)
            throw new BusinessRuleException("This tax return has already been fully settled. Outstanding balance is 0.00.");

        // Rule 4: Anti-Overpayment Policy.
        // A government revenue portal must NEVER accept more money than is legally assessed.
        // Overpayments create complex audit reconciliations, accounting liabilities, and citizen disputes.
        if (request.Amount > outstanding)
            throw new BusinessRuleException(
                $"Payment of {request.Amount:N2} exceeds the outstanding {outstanding:N2}.");

        // Rule 5 & 6: Idempotency and External Banking Settlement Verification.
        // In high-volume financial applications, idempotency prevents double billing when users
        // accidentally double-click "Submit" or mobile networks retry a timed-out request.
        if (!string.IsNullOrWhiteSpace(request.Reference))
        {
            // Idempotency check: has this specific bank reference already been credited in our database?
            if (await payments.ExistsByReferenceAsync(request.Reference))
            {
                throw new BusinessRuleException($"Transaction reference {request.Reference} has already been processed.");
            }

            // Settlement check: query the gateway to ensure funds were cleared, not reversed or fraudulent.
            bool verified = await gateway.VerifyTransactionAsync(request.Reference, request.Amount, cancellationToken);
            if (!verified)
            {
                throw new BusinessRuleException("Payment gateway could not confirm settlement for this transaction.");
            }
        }

        // All rules validated! Proceed to persist the financial transaction into SQL Server.
        var paymentId = await payments.SaveAsync(request);

        // Structured Logging: This log entry is captured by operations monitoring.
        logger.LogInformation("Payment {PaymentId} recorded for return {ReturnId}", paymentId, request.ReturnId);

        // Official Receipt Generation:
        // Format as 'RCT-' followed by the 6-digit zero-padded ID (e.g. PaymentId 5 -> RCT-000005).
        return new PaymentResult(paymentId, $"RCT-{paymentId:D6}", "Successful");
    }
}
