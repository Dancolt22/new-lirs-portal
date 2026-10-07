using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxpayersController(TaxpayerRepository repo) : ControllerBase
{
    // GET /api/taxpayers?tin=1000000001&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetAll(string? tin, int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await repo.GetPageAsync(tin, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var taxpayer = await repo.GetByIdAsync(id);
        return taxpayer is null
            ? NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"))
            : Ok(taxpayer);
    }

    [HttpGet("{id}/returns")]
    public async Task<IActionResult> GetReturns(int id)
    {
        if (await repo.GetByIdAsync(id) is null)
            return NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"));
        return Ok(await repo.GetReturnsAsync(id));
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(int id)
    {
        if (await repo.GetByIdAsync(id) is null)
            return NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"));
        return Ok(new BalanceResult(id, await repo.GetBalanceAsync(id)));
    }
}
