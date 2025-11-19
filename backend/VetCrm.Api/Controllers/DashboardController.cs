using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Infrastructure.Data;
using VetCrm.Domain.Entities; // Reminder, Visit, Pet, Owner burada

namespace VetCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public DashboardController(VetCrmDbContext db)
    {
        _db = db;
    }

    // ÖZET KISMI (3 kutu)
    [HttpGet("reminders-summary")]
    public async Task<ActionResult<ReminderSummaryDto>> GetRemindersSummary()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

        const int STATUS_PENDING = 0;

        var pendingToday = await _db.Reminders
            .Where(r => r.DueDate == today && r.Status == STATUS_PENDING)
            .CountAsync();

        var pendingTomorrow = await _db.Reminders
            .Where(r => r.DueDate == tomorrow && r.Status == STATUS_PENDING)
            .CountAsync();

        var overdue = await _db.Reminders
            .Where(r => r.DueDate < today && r.Status == STATUS_PENDING)
            .CountAsync();

        // Altta gösterilecek default liste: upcoming
        var upcoming = await _db.Reminders
            .Where(r => r.DueDate > today && r.Status == STATUS_PENDING)
            .OrderBy(r => r.DueDate)
            .Take(5)
            .Include(r => r.Visit)!.ThenInclude(v => v!.Pet)!.ThenInclude(p => p!.Owner)
            .Select(r => new ReminderItemDto
            {
                Id = r.Id,
                VisitId = r.VisitId,
                ReminderDate = r.DueDate,
                AppointmentDate = r.Visit!.NextDate ?? r.DueDate,
                PetName = r.Visit!.Pet!.Name,
                OwnerName = r.Visit!.Pet!.Owner!.FullName,
                Procedures = r.Visit!.Procedures ?? string.Empty
            })
            .ToListAsync();

        var dto = new ReminderSummaryDto
        {
            PendingToday = pendingToday,
            PendingTomorrow = pendingTomorrow,
            Overdue = overdue,
            Upcoming = upcoming
        };

        return Ok(dto);
    }

    // KARTLARA TIKLANINCA KULLANILACAK DETAY LİSTE
    [HttpGet("reminders")]
    public async Task<ActionResult<List<ReminderItemDto>>> GetReminders([FromQuery] string filter = "upcoming")
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

        const int STATUS_PENDING = 0;

        IQueryable<Reminder> query = _db.Reminders
            .Where(r => r.Status == STATUS_PENDING);

        switch (filter.ToLowerInvariant())
        {
            case "today":
                query = query.Where(r => r.DueDate == today);
                break;
            case "tomorrow":
                query = query.Where(r => r.DueDate == tomorrow);
                break;
            case "overdue":
                query = query.Where(r => r.DueDate < today);
                break;
            default: // upcoming
                query = query.Where(r => r.DueDate > today);
                break;
        }

        var items = await query
            .OrderBy(r => r.DueDate)
            .Include(r => r.Visit)!.ThenInclude(v => v!.Pet)!.ThenInclude(p => p!.Owner)
            .Select(r => new ReminderItemDto
            {
                Id = r.Id,
                VisitId = r.VisitId,
                ReminderDate = r.DueDate,
                AppointmentDate = r.Visit!.NextDate ?? r.DueDate,
                PetName = r.Visit!.Pet!.Name,
                OwnerName = r.Visit!.Pet!.Owner!.FullName,
                Procedures = r.Visit!.Procedures ?? string.Empty
            })
            .ToListAsync();

        return Ok(items);
    }

// ZİYARET DETAYI (modal için)
[HttpGet("visit/{id:int}")]
public async Task<ActionResult<VisitDetailDto>> GetVisitDetail(int id)
{
    var v = await _db.Visits
        .Include(x => x.Pet)!.ThenInclude(p => p.Owner)
        .FirstOrDefaultAsync(x => x.Id == id);

    if (v == null)
        return NotFound();

    var dto = new VisitDetailDto
    {
        Id = v.Id,
        PetName = v.Pet!.Name,
        OwnerName = v.Pet.Owner!.FullName,
        PerformedAt = v.PerformedAt,
         NextDate = v.NextDate.HasValue
        ? v.NextDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Zero))
        : (DateTime?)null,

        // 🔥 burada ne için gelecek alanını da dolduruyoruz
        Purpose = v.Purpose,

        Procedures = v.Procedures ?? string.Empty,
        AmountTl = v.AmountTl,
        Notes = v.Notes ?? string.Empty,
        ImageUrl = v.ImageUrl
    };

    return Ok(dto);
}



}
