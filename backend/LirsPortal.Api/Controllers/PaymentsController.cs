using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using LirsPortal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

/// <summary>
/// HTTP API Controller responsible for receiving tax payment submissions.
/// 
/// Security & Integrity Defenses Implemented:
/// 1. Mandatory Authentication: Requires a valid JWT bearer token via [Authorize].
/// 2. Taxpayer Ownership Enforcement:
///    - When a citizen attempts to make a payment against a tax return, the controller
///      verifies that the return actually belongs to their TaxpayerId. Citizens cannot
///      falsely pay or manipulate assessments belonging to another citizen.
/// 3. Idempotency & Financial Resilience:
///    - Passes the payment request and cancellation token through to PaymentService,
///      which validates banking settlement and rejects duplicate reference IDs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController(PaymentService service, IPaymentRepository payments) : ControllerBase
{
    /// <summary>
    /// POST /api/payments
    /// Receives a payment payload from the React frontend, validates ownership and business rules,
    /// and records the payment into SQL Server.
    /// </summary>
    /// <param name="request">JSON object containing ReturnId, Amount, Channel, and optional Reference</param>
    /// <param name="cancellationToken">Allows aborting if the client disconnects mid-flight</param>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PaymentRequest request, CancellationToken cancellationToken)
    {
        // Ownership Check: If the caller is a citizen (not an Officer),
        // verify that the tax return being paid against legally belongs to them.
        if (!User.IsInRole("Officer"))
        {
            var userTaxpayerIdClaim = User.FindFirst("taxpayerId")?.Value;
            var taxReturn = await payments.GetReturnAsync(request.ReturnId);
            if (taxReturn is not null && int.TryParse(userTaxpayerIdClaim, out int userTaxpayerId))
            {
                if (taxReturn.TaxpayerId != userTaxpayerId)
                {
                    return StatusCode(403, new ApiError("You cannot record a payment against another taxpayer's return.", "FORBIDDEN"));
                }
            }
        }

        try
        {
            // Delegate statutory policy validation and gateway settlement to PaymentService
            var result = await service.RecordPaymentAsync(request, cancellationToken);

            // HTTP 201 Created:
            // Standard REST practice: return HTTP 201 with official receipt payload
            return Created($"/api/payments/{result.PaymentId}", result);
        }
        catch (NotFoundException ex)
        {
            // The return ID does not exist in the database -> HTTP 404
            return NotFound(new ApiError(ex.Message, "NOT_FOUND"));
        }
        catch (BusinessRuleException ex)
        {
            // The payment violates a policy (e.g. overpayment, draft return, duplicate reference) -> HTTP 400 Bad Request
            return BadRequest(new ApiError(ex.Message, "BUSINESS_RULE"));
        }
    }
}
