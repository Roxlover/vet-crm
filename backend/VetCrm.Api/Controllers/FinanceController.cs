using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FinanceController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public FinanceController(VetCrmDbContext db)
    {
        _db = db;
    }

    private int GetUserId()
    {
        // JWT’de NameIdentifier claim’inden alıyoruz
        var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(s) || !int.TryParse(s, out var id))
            throw new InvalidOperationException("UserId claim not found.");
        return id;
    }

    [HttpGet("me/summary")]
    public async Task<IActionResult> GetMySummary()
    {
        var userId = GetUserId();

        var income = await _db.LedgerEntries
            .Where(x => x.UserId == userId && x.IsIncome)
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        var expense = await _db.LedgerEntries
            .Where(x => x.UserId == userId && !x.IsIncome)
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        return Ok(new
        {
            income,
            expense,
            net = income - expense
        });
    }

    [HttpGet("me/entries")]
    public async Task<IActionResult> GetMyEntries([FromQuery] int take = 50)
    {
        var userId = GetUserId();
        take = Math.Clamp(take, 1, 200);

        var list = await _db.LedgerEntries
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new
            {
                x.Id,
                x.Date,
                x.Amount,
                x.IsIncome,
                x.Category,
                x.Note,
                x.VisitId,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }
}
