using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class LedgerController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public LedgerController(VetCrmDbContext db)
    {
        _db = db;
    }

    private static (decimal total, decimal collected, decimal credit) CalcAmounts(
        decimal? amount, decimal? credit)
    {
        var total = amount ?? 0m;
        var creditVal = credit ?? 0m;
        var collected = total - creditVal;
        if (collected < 0) collected = 0m;

        return (total, collected, creditVal);
    }

    public class LedgerUserGroupDto
    {
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }

        public LedgerSummaryDto Summary { get; set; } = new();

        public List<LedgerVisitItemDto> Items { get; set; } = new();
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
    public class LedgerSummaryDto
    {
        public decimal TotalAmount { get; set; }    
        public decimal TotalCollected { get; set; }  
        public decimal TotalCredit { get; set; }     
        public int VisitCount { get; set; }       
    }

    public class LedgerVisitItemDto
    {
        public int VisitId { get; set; }
        public DateTime PerformedAt { get; set; }

        public string PetName { get; set; } = null!;
        public string OwnerName { get; set; } = null!;
        public string? OwnerPhoneE164 { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal CreditAmount { get; set; }

        public string? CreatedByUsername { get; set; }
        public string? CreatedByName { get; set; }
    }


    [HttpGet]
    public async Task<ActionResult<List<LedgerEntryDto>>> GetRange(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        var list = await _db.LedgerEntries
            .Where(l => l.Date >= from && l.Date <= to)
            .OrderBy(l => l.Date)
            .ThenBy(l => l.Id)
            .Select(l => new LedgerEntryDto
            {
                Id = l.Id,
                Date = l.Date,
                Amount = l.Amount,
                IsIncome = l.IsIncome,
                Category = l.Category,
                Note = l.Note
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<LedgerEntryDto>> Create(
        [FromBody] CreateLedgerEntryRequest request)
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
            CreatedAt = DateTime.UtcNow
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

[HttpGet("summary")]
public async Task<ActionResult<LedgerSummaryDto>> GetVisitSummary(
    [FromQuery] DateOnly from,
    [FromQuery] DateOnly to)
{
    if (to < from)
    {
        var tmp = from;
        from = to;
        to = tmp;
    }

    var query = _db.Visits
        .Where(v =>
            DateOnly.FromDateTime(v.PerformedAt.Date) >= from &&
            DateOnly.FromDateTime(v.PerformedAt.Date) <= to);

    query = query.Where(v =>
        _db.Reminders.Any(r => r.VisitId == v.Id && r.IsCompleted));

    var visits = await query
        .Select(v => new
        {
            v.AmountTl,
            v.CreditAmountTl
        })
        .ToListAsync();

    decimal totalAmount = 0m;
    decimal totalCollected = 0m;
    decimal totalCredit = 0m;

    foreach (var v in visits)
    {
        var (t, c, cr) = CalcAmounts(v.AmountTl, v.CreditAmountTl);
        totalAmount += t;
        totalCollected += c;
        totalCredit += cr;
    }

    var dto = new LedgerSummaryDto
    {
        TotalAmount = totalAmount,
        TotalCollected = totalCollected,
        TotalCredit = totalCredit,
        VisitCount = visits.Count
    };

    return Ok(dto);
}

[HttpGet("visit-items")]
public async Task<ActionResult<List<LedgerVisitItemDto>>> GetVisitItems(
    [FromQuery] DateOnly from,
    [FromQuery] DateOnly to)
{
    if (to < from)
    {
        var tmp = from;
        from = to;
        to = tmp;
    }

    var baseQuery = _db.Visits
        .Include(v => v.Pet)
            .ThenInclude(p => p.Owner)
        .Where(v =>
            DateOnly.FromDateTime(v.PerformedAt.Date) >= from &&
            DateOnly.FromDateTime(v.PerformedAt.Date) <= to);

    baseQuery = baseQuery.Where(v =>
        _db.Reminders.Any(r => r.VisitId == v.Id && r.IsCompleted));

    var data = await baseQuery
        .Select(v => new
        {
            v.Id,
            v.PerformedAt,
            v.AmountTl,
            v.CreditAmountTl,
            PetName = v.Pet.Name,
            OwnerName = v.Pet.Owner.FullName,
            v.Pet.Owner.PhoneE164,
            v.CreatedByUsername,
            v.CreatedByName
        })
        .ToListAsync();

    var result = data
        .Select(v =>
        {
            var (total, collected, credit) = CalcAmounts(v.AmountTl, v.CreditAmountTl);
            return new LedgerVisitItemDto
            {
                VisitId = v.Id,
                PerformedAt = v.PerformedAt,
                PetName = v.PetName,
                OwnerName = v.OwnerName,
                OwnerPhoneE164 = v.PhoneE164,
                TotalAmount = total,
                CollectedAmount = collected,
                CreditAmount = credit,
                CreatedByUsername = v.CreatedByUsername,
                CreatedByName = v.CreatedByName
            };
        })
        .OrderByDescending(x => x.PerformedAt)
        .ThenBy(x => x.OwnerName)
        .ThenBy(x => x.PetName)
        .ToList();

    return Ok(result);
}



[HttpGet("by-user")]
public async Task<ActionResult<List<LedgerUserGroupDto>>> GetByUser(
    [FromQuery] DateOnly from,
    [FromQuery] DateOnly to)
{
    if (to < from)
    {
        var tmp = from;
        from = to;
        to = tmp;
    }

    var visits = await _db.Visits
        .Include(v => v.Pet)
            .ThenInclude(p => p.Owner)
        .Where(v =>
            DateOnly.FromDateTime(v.PerformedAt.Date) >= from &&
            DateOnly.FromDateTime(v.PerformedAt.Date) <= to)
        .ToListAsync();

    var groups = visits
        .GroupBy(v => new
        {
            v.CreatedByUserId,
            v.CreatedByUsername,
            v.CreatedByName
        })
        .Select(g =>
        {
            decimal totalAmount = 0m;
            decimal totalCollected = 0m;
            decimal totalCredit = 0m;

            var items = g.Select(v =>
            {
                var (total, collected, credit) = CalcAmounts(v.AmountTl, v.CreditAmountTl);
                totalAmount += total;
                totalCollected += collected;
                totalCredit += credit;

                return new LedgerVisitItemDto
                {
                    VisitId = v.Id,
                    PerformedAt = v.PerformedAt,
                    PetName = v.Pet!.Name,
                    OwnerName = v.Pet.Owner!.FullName,
                    OwnerPhoneE164 = v.Pet.Owner.PhoneE164,
                    TotalAmount = total,
                    CollectedAmount = collected,
                    CreditAmount = credit,
                    CreatedByUsername = v.CreatedByUsername,
                    CreatedByName = v.CreatedByName
                };
            })
            .OrderByDescending(x => x.PerformedAt)
            .ThenBy(x => x.OwnerName)
            .ThenBy(x => x.PetName)
            .ToList();

            return new LedgerUserGroupDto
            {
                UserId = g.Key.CreatedByUserId,
                Username = g.Key.CreatedByUsername,
                FullName = g.Key.CreatedByName,
                Summary = new LedgerSummaryDto
                {
                    TotalAmount = totalAmount,
                    TotalCollected = totalCollected,
                    TotalCredit = totalCredit,
                    VisitCount = items.Count
                },
                Items = items
            };
        })
        .OrderBy(g => g.UserId.HasValue ? 0 : 1)
        .ThenBy(g => g.FullName ?? g.Username)
        .ToList();

    return Ok(groups);
}
}
