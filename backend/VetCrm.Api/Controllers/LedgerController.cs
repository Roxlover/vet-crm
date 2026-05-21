using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;
using System.Security.Claims;

namespace VetCrm.Api.Controllers;

[Authorize(Roles = "Admin,BullBoss")]
[ApiController]
[Route("api/[controller]")]
public class LedgerController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public LedgerController(VetCrmDbContext db)
    {
        _db = db;
    }

private static (decimal total, decimal collected, decimal credit) CalcAmounts(decimal? amount, decimal? credit, decimal? collectedActual)
{
    var total = amount ?? 0m;
    var creditVal = credit ?? 0m;
    
    // Eğer collectedActual (LedgerEntries'den gelen gerçek tahsilat) varsa onu kullan, 
    // yoksa (eski veri veya henüz senkronize olmamışsa) Amount - Credit yap.
    var collected = collectedActual ?? (total - creditVal);

    return (total, collected, creditVal);
}


private static bool HasWork(Visit v)
{
    return (v.AmountTl ?? 0m) > 0m
        || !string.IsNullOrWhiteSpace(v.Procedures)
        || !string.IsNullOrWhiteSpace(v.Purpose)
        || !string.IsNullOrWhiteSpace(v.Notes);
}

private IQueryable<Visit> ApplyLedgerInclusionRule(IQueryable<Visit> q)
{
    return q.Where(v =>
        (v.AmountTl ?? 0m) > 0m
        || v.Procedures != null && v.Procedures != ""
        || v.Purpose != null && v.Purpose != ""
        || v.Notes != null && v.Notes != ""
    );
}

// Türkiye UTC+3 offset'ini hesaba katarak gün sınırlarını UTC'ye çevirir
private static (DateTime fromDt, DateTime toDt) ToUtcRange(DateOnly from, DateOnly to)
{
    // UTC+3: gün başlangıcı = 00:00 yerel = önceki gün 21:00 UTC
    //        gün sonu       = 23:59 yerel = aynı gün 20:59 UTC
    // Güvenli taraf: ±1 gün buffer ile tüm gün aralığını yakala
    var fromDt = from.ToDateTime(TimeOnly.MinValue).AddHours(-3); // UTC'de güvenli başlangıç
    var toDt   = to.ToDateTime(TimeOnly.MaxValue).AddHours(0);     // UTC'de güvenli bitiş (23:59 yerel = 20:59 UTC, MaxValue zaten kapsar)
    return (fromDt, toDt);
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
        public string? Purpose { get; set; }
        public string? Procedures { get; set; }
        public string? Notes { get; set; }    
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
  
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
        return Unauthorized("UserId claim bulunamadı."); 

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
            CreatedAt = DateTime.UtcNow,
            UserId = userId
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

    var (fromDt, toDt) = ToUtcRange(from, to);

    var visitQuery = _db.Visits
        .Where(v => v.PerformedAt >= fromDt && v.PerformedAt <= toDt);
    visitQuery = ApplyLedgerInclusionRule(visitQuery);
    
    var visitsList = await visitQuery.ToListAsync();

    var visitRevenue = visitsList.Sum(v => v.AmountTl ?? 0m);
    var visitCollected = visitsList.Sum(v => v.CollectedAmountTl ?? Math.Max(0m, (v.AmountTl ?? 0m) - (v.CreditAmountTl ?? 0m)));

    var manualIncomes = await _db.LedgerEntries
        .Where(l => l.Date >= from && l.Date <= to && l.IsIncome && l.Category != "VisitIncome" && l.Category != "VisitCollected")
        .SumAsync(l => l.Amount);

    var totalRevenue = visitRevenue + manualIncomes;
    var totalCollected = visitCollected + manualIncomes;

    var dto = new LedgerSummaryDto
    {
        TotalAmount = totalRevenue,
        TotalCollected = totalCollected,
        TotalCredit = Math.Max(0, totalRevenue - totalCollected),
        VisitCount = visitsList.Count
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

    var (fromDt, toDt) = ToUtcRange(from, to);

    var baseQuery = _db.Visits
        .Include(v => v.Pet)
            .ThenInclude(p => p.Owner)
        .Where(v => v.PerformedAt >= fromDt && v.PerformedAt <= toDt);
    baseQuery = ApplyLedgerInclusionRule(baseQuery);

    var data = await baseQuery
        .Select(v => new
        {
            v.Id,
            v.PerformedAt,
            v.AmountTl,
            v.CreditAmountTl,
            v.CollectedAmountTl,
            PetName = v.Pet.Name,
            OwnerName = v.Pet.Owner.FullName,
            v.Pet.Owner.PhoneE164,
            v.CreatedByUsername,
            v.CreatedByName,
            v.Purpose,
            v.Procedures,
            v.Notes
        })
        .ToListAsync();

    var result = data
        .Select(v =>
        {
            var (total, collected, credit) = CalcAmounts(v.AmountTl, v.CreditAmountTl, v.CollectedAmountTl);
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
                CreatedByName = v.CreatedByName,
                Purpose = v.Purpose,
                Procedures = v.Procedures,
                Notes = v.Notes
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

        var (fromDt, toDt) = ToUtcRange(from, to);

        // 1) Get all visits in range
        var visitQuery = _db.Visits
            .Where(v => v.PerformedAt >= fromDt && v.PerformedAt <= toDt);
        visitQuery = ApplyLedgerInclusionRule(visitQuery);
        
        var visitsRaw = await visitQuery
            .Select(v => new
            {
                v.Id,
                v.CreatedByUserId,
                v.CreatedByUsername,
                v.CreatedByName,
                v.PerformedAt,
                v.AmountTl,
                v.CreditAmountTl,
                v.CollectedAmountTl,
                PetName = v.Pet != null ? v.Pet.Name : "—",
                OwnerName = (v.Pet != null && v.Pet.Owner != null) ? v.Pet.Owner.FullName : "—",
                OwnerPhone = (v.Pet != null && v.Pet.Owner != null) ? v.Pet.Owner.PhoneE164 : null,
                v.Purpose,
                v.Procedures,
                v.Notes
            })
            .ToListAsync();

        // 2) Get all ledger entries in range to calculate real totals
        var ledgerEntries = await _db.LedgerEntries
            .Where(l => l.Date >= from && l.Date <= to && l.IsIncome && l.Category != "VisitIncome" && l.Category != "VisitCollected")
            .ToListAsync();

        // 3) Group and Prepare Results
        var grouped = visitsRaw.GroupBy(v => v.CreatedByUserId ?? 0);
        var results = new List<LedgerUserGroupDto>();

        foreach (var g in grouped)
        {
            var userId = g.Key;
            var first = g.First();

            // Filter manual incomes for this user
            var userLedger = userId == 0
                ? ledgerEntries.Where(l => l.UserId == 0).ToList()
                : ledgerEntries.Where(l => l.UserId == userId).ToList();
            
            var manualIncomeSum = userLedger.Sum(l => l.Amount);
            
            var visitRevenue = g.Sum(v => v.AmountTl ?? 0m);
            var visitCollected = g.Sum(v => v.CollectedAmountTl ?? Math.Max(0m, (v.AmountTl ?? 0m) - (v.CreditAmountTl ?? 0m)));

            var totalAmount = visitRevenue + manualIncomeSum;
            var totalCollected = visitCollected + manualIncomeSum;
            
            var totalCredit = Math.Max(0, totalAmount - totalCollected);

            var items = g.Select(v =>
            {
                var (total, collected, credit) = CalcAmounts(v.AmountTl, v.CreditAmountTl, v.CollectedAmountTl);
                return new LedgerVisitItemDto
                {
                    VisitId = v.Id,
                    PerformedAt = v.PerformedAt,
                    PetName = v.PetName,
                    OwnerName = v.OwnerName,
                    OwnerPhoneE164 = v.OwnerPhone,
                    TotalAmount = total,
                    CollectedAmount = collected,
                    CreditAmount = credit,
                    CreatedByUsername = v.CreatedByUsername,
                    CreatedByName = v.CreatedByName,
                    Purpose = v.Purpose,
                    Procedures = v.Procedures,
                    Notes = v.Notes
                };
            })
            .OrderByDescending(x => x.PerformedAt)
            .ToList();

            results.Add(new LedgerUserGroupDto
            {
                UserId = userId == 0 ? null : userId,
                Username = first.CreatedByUsername ?? "Sistem",
                FullName = first.CreatedByName ?? "Sistem / Manuel",
                Summary = new LedgerSummaryDto
                {
                    TotalAmount = totalAmount,
                    TotalCollected = totalCollected,
                    TotalCredit = totalCredit,
                    VisitCount = items.Count
                },
                Items = items
            });
        }

        return Ok(results.OrderBy(r => r.UserId.HasValue ? 0 : 1).ThenBy(r => r.FullName).ToList());
    }
}
