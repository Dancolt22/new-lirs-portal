using System.Security.Claims;
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

/// <summary>
/// HTTP API Controller exposing taxpayer directory, balance, and return history endpoints.
/// 
/// Security Defense Patterns Implemented:
/// 1. OWASP A01: Broken Access Control / Insecure Direct Object References (IDOR) Defense:
///    - Simply authenticating a user is NOT enough. A logged-in citizen with ID 101 must NEVER
///      be able to view the balance, profile, or returns of citizen ID 102 just by tampering with the URL.
///    - The IsAuthorizedForTaxpayer check enforces that citizens can only access their OWN data.
/// 2. Role-Based Access Control (RBAC):
///    - Directory listing (GetAll) is strictly restricted to authenticated revenue officers
///      via [Authorize(Roles = "Officer")].
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaxpayersController(TaxpayerRepository repo) : ControllerBase
{
    /// <summary>
    /// GET /api/taxpayers?tin=1000000001&page=1&pageSize=20
    /// Returns a paginated directory of registered taxpayers, with optional TIN search filter.
    /// Restricted exclusively to revenue officers.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Officer")]
    public async Task<IActionResult> GetAll([FromQuery] string? tin, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Defensive Engineering (Clamping):
        // Never allow a client to request pageSize = 1,000,000 or negative pages!
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var taxpayers = await repo.GetPageAsync(tin, page, pageSize);
        return Ok(taxpayers);
    }

    /// <summary>
    /// GET /api/taxpayers/101
    /// Retrieves full profile details for a specific taxpayer by their ID.
    /// Enforces taxpayer ownership check: Taxpayer 101 cannot view Taxpayer 102.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Ownership Guard: Returns 403 Forbidden if a citizen attempts to inspect another citizen's file
        if (!IsAuthorizedForTaxpayer(id))
            return StatusCode(403, new ApiError("Access denied to requested taxpayer profile.", "FORBIDDEN"));

        var taxpayer = await repo.GetByIdAsync(id);

        return taxpayer is null
            ? NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"))
            : Ok(taxpayer);
    }

    /// <summary>
    /// GET /api/taxpayers/101/returns
    /// Retrieves all filed tax returns for a specific citizen.
    /// </summary>
    [HttpGet("{id}/returns")]
    public async Task<IActionResult> GetReturns(int id)
    {
        // Ownership Guard
        if (!IsAuthorizedForTaxpayer(id))
            return StatusCode(403, new ApiError("Access denied to requested taxpayer returns.", "FORBIDDEN"));

        if (await repo.GetByIdAsync(id) is null)
            return NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"));

        var returns = await repo.GetReturnsAsync(id);
        return Ok(returns);
    }

    /// <summary>
    /// GET /api/taxpayers/101/balance
    /// Calculates and returns the live outstanding tax liability in Naira.
    /// </summary>
    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(int id)
    {
        // Ownership Guard
        if (!IsAuthorizedForTaxpayer(id))
            return StatusCode(403, new ApiError("Access denied to requested taxpayer balance.", "FORBIDDEN"));

        if (await repo.GetByIdAsync(id) is null)
            return NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"));

        var balance = await repo.GetBalanceAsync(id);
        return Ok(new BalanceResult(id, balance));
    }

    /// <summary>
    /// Ownership validation helper method:
    /// - Officers hold administrative privilege and can inspect any citizen record.
    /// - Citizens can ONLY access data matching the 'taxpayerId' claim stamped into their verified JWT.
    /// </summary>
    private bool IsAuthorizedForTaxpayer(int requestedTaxpayerId)
    {
        if (User.IsInRole("Officer")) return true;

        var claim = User.FindFirst("taxpayerId")?.Value;
        return int.TryParse(claim, out int userTaxpayerId) && userTaxpayerId == requestedTaxpayerId;
    }
}
