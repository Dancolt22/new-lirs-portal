using LirsPortal.Api.Models;
using LirsPortal.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(PaymentService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(PaymentRequest request)
    {
        try
        {
            var result = await service.RecordPaymentAsync(request);
            return Created($"/api/payments/{result.PaymentId}", result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiError(ex.Message, "NOT_FOUND"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiError(ex.Message, "BUSINESS_RULE"));
        }
    }
}
