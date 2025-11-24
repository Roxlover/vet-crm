using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LedgerController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public LedgerController(VetCrmDbContext db)
    {
        _db = db;
    }

    public class LedgerEntryDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncome { get; set; }
        public string? Category { get; set; }
        public string? Note { get; set; }
    }

    public class CreateLedgerEntryRequest
    {
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncome { get; set; }
        public string? Category { get; set; }
        public string? Note { get; set; }
    }

    [HttpGet]
    public async Task<ActionResult<List<LedgerEntryDto>>> GetRange(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        var list = await _db.LedgerEntries
            .Where(x => x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ThenByDescending(x => x.Id)
            .Select(x => new LedgerEntryDto
            {
                Id = x.Id,
                Date = x.Date,
                Amount = x.Amount,
                IsIncome = x.IsIncome,
                Category = x.Category,
                Note = x.Note
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<LedgerEntryDto>> Create([FromBody] CreateLedgerEntryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entry = new LedgerEntry
        {
            Date = request.Date,
            Amount = request.Amount,
            IsIncome = request.IsIncome,
            Category = string.IsNullOrWhiteSpace(request.Category)
                ? null
                : request.Category.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Note)
                ? null
                : request.Note.Trim(),
            CreatedAt = DateTime.UtcNow      // 🔴 burada da elle set ediyoruz
        };

        _db.LedgerEntries.Add(entry);
        await _db.SaveChangesAsync();

        var dto = new LedgerEntryDto
        {
            Id = entry.Id,
            Date = entry.Date,
            Amount = entry.Amount,
            IsIncome = entry.IsIncome,
            Category = entry.Category,
            Note = entry.Note
        };

        return Ok(dto);
    }
}
