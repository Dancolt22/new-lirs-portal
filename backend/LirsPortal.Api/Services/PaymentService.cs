using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;

namespace LirsPortal.Api.Services;

public class PaymentService(PaymentRepository payments, ILogger<PaymentService> logger)
{
    public async Task<PaymentResult> RecordPaymentAsync(PaymentRequest request)
    {
        var taxReturn = await payments.GetReturnAsync(request.ReturnId)
            ?? throw new NotFoundException("Return not found.");

        if (taxReturn.Status == "Draft")
            throw new BusinessRuleException("Payments cannot be recorded against a draft return.");

        var alreadyPaid = await payments.GetTotalPaidAsync(request.ReturnId);
        var outstanding = taxReturn.TaxDue - alreadyPaid;

        // A payment must never exceed what is still owed on the return
        if (request.Amount > outstanding)
            throw new BusinessRuleException(
                $"Payment of {request.Amount:N2} exceeds the outstanding {outstanding:N2}.");

        var paymentId = await payments.SaveAsync(request);
        logger.LogInformation("Payment {PaymentId} recorded for return {ReturnId}", paymentId, request.ReturnId);

        return new PaymentResult(paymentId, $"RCT-{paymentId:D6}", "Successful");
    }
}
