using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Infrastructure.Data;
using VetCrm.Domain.Entities;

namespace VetCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public DiagnosticsController(VetCrmDbContext db)
    {
        _db = db;
    }

    [HttpGet("db-check")]
    public async Task<IActionResult> CheckDb()
    {
        var visitCount = await _db.Visits.CountAsync();
        var ledgerCount = await _db.LedgerEntries.CountAsync();
        
        var sampleVisits = await _db.Visits
            .OrderByDescending(v => v.PerformedAt)
            .Take(5)
            .Select(v => new { v.Id, v.AmountTl, v.CreditAmountTl, v.CollectedAmountTl, v.PerformedAt, v.Status })
            .ToListAsync();
            
        var sampleLedgers = await _db.LedgerEntries
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .Select(l => new { l.Id, l.Amount, l.IsIncome, l.Category, l.Date })
            .ToListAsync();

        return Ok(new
        {
            VisitCount = visitCount,
            LedgerCount = ledgerCount,
            SampleVisits = sampleVisits,
            SampleLedgers = sampleLedgers
        });
    }

    [HttpPost("sync-ledger")]
    public async Task<IActionResult> SyncLedger()
    {
        var visits = await _db.Visits.ToListAsync();

        int syncedCount = 0;
        foreach (var v in visits)
        {
            // 1. VisitIncome
            if ((v.AmountTl ?? 0m) > 0m)
            {
                var hasIncome = await _db.LedgerEntries.AnyAsync(l => l.VisitId == v.Id && l.Category == "VisitIncome");
                if (!hasIncome)
                {
                    _db.LedgerEntries.Add(new LedgerEntry
                    {
                        UserId = v.CreatedByUserId ?? 1,
                        VisitId = v.Id,
                        Date = DateOnly.FromDateTime(v.PerformedAt.Date),
                        Amount = v.AmountTl ?? 0m,
                        IsIncome = true,
                        Category = "VisitIncome",
                        Note = "[Sync] Ziyaret Tahakkuk",
                        CreatedAt = DateTime.UtcNow
                    });
                    syncedCount++;
                }
            }

            // 2. VisitCollected
            if ((v.CollectedAmountTl ?? 0m) > 0m)
            {
                var hasColl = await _db.LedgerEntries.AnyAsync(l => l.VisitId == v.Id && l.Category == "VisitCollected");
                if (!hasColl)
                {
                    _db.LedgerEntries.Add(new LedgerEntry
                    {
                        UserId = v.CreatedByUserId ?? 1,
                        VisitId = v.Id,
                        Date = DateOnly.FromDateTime(v.PerformedAt.Date),
                        Amount = v.CollectedAmountTl ?? 0m,
                        IsIncome = true,
                        Category = "VisitCollected",
                        Note = "[Sync] Ziyaret Tahsilat",
                        CreatedAt = DateTime.UtcNow
                    });
                    syncedCount++;
                }
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { Message = $"{syncedCount} ledger entries created/synced.", TotalVisitsChecked = visits.Count });
    }
}
