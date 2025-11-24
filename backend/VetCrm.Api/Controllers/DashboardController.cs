using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Infrastructure.Data;
using VetCrm.Domain.Entities; // Reminder, Visit, Pet, Owner

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

    // === Helper: reminder'ı dashboard DTO'ya çevir ===
    private static ReminderDashboardDto MapToDashboardDto(Reminder r)
    {
        return new ReminderDashboardDto
        {
            Id = r.Id,
            OwnerName = r.Visit!.Pet!.Owner!.FullName,
            OwnerPhone = r.Visit!.Pet!.Owner!.PhoneE164,
            PetName = r.Visit!.Pet!.Name,
            DueDate = r.DueDate,
            Procedures = r.Visit!.Procedures ?? string.Empty,
            IsCompleted = r.IsCompleted,          // bu alanı entity'ne eklediğini varsayıyorum
            VisitImageUrl = r.Visit!.ImageUrl     // ziyarette sakladığın görsel alanı
        };
    }

[HttpGet("reminders-summary")]
public async Task<ActionResult<ReminderSummaryDto>> GetRemindersSummary()
{
    var today = DateOnly.FromDateTime(DateTime.Today);
    var tomorrow = today.AddDays(1);

    // Pending sayıları (IsCompleted = false)
    var pendingToday = await _db.Reminders
        .Where(r => r.DueDate == today && !r.IsCompleted)
        .CountAsync();

    var pendingTomorrow = await _db.Reminders
        .Where(r => r.DueDate == tomorrow && !r.IsCompleted)
        .CountAsync();

    var overdue = await _db.Reminders
        .Where(r => r.DueDate < today && !r.IsCompleted)
        .CountAsync();

    var completed = await _db.Reminders
        .Where(r => r.IsCompleted)
        .CountAsync();

    // Aşağıdaki default liste: upcoming (bugünden sonrası, tamamlanmamış)
    var upcoming = await _db.Reminders
        .Where(r => r.DueDate > today && !r.IsCompleted)
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
        Completed = completed,   // kartta kullandığın alan
        Upcoming = upcoming
    };

    return Ok(dto);
}

   [HttpGet("reminders")]
public async Task<ActionResult<List<ReminderItemDto>>> GetReminders(
    [FromQuery] string filter = "upcoming")
{
    var today = DateOnly.FromDateTime(DateTime.Today);
    var tomorrow = today.AddDays(1);

    IQueryable<Reminder> query = _db.Reminders.AsQueryable();

    switch (filter.ToLowerInvariant())
    {
        case "today":
            // Bugün + tamamlanmamış
            query = query.Where(r => r.DueDate == today && !r.IsCompleted);
            break;

        case "tomorrow":
            // Yarın + tamamlanmamış
            query = query.Where(r => r.DueDate == tomorrow && !r.IsCompleted);
            break;

        case "overdue":
            // Bugünden önce + tamamlanmamış
            query = query.Where(r => r.DueDate < today && !r.IsCompleted);
            break;

        case "done":
            // Sadece tamamlanmış kayıtlar
            query = query.Where(r => r.IsCompleted)
                         .OrderByDescending(r => r.CompletedAt);
            break;

        default: // "upcoming"
            // Bugünden sonrası + tamamlanmamış
            query = query.Where(r => r.DueDate > today && !r.IsCompleted);
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

    // ============= YENİ: DASHBOARD 4 LİSTE (bugün / yarın / geciken / yapıldı) =============
    [HttpGet("reminders-dashboard")]
    public async Task<ActionResult<ReminderDashboardResponse>> GetRemindersDashboard()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

        // tüm reminder'ları ilişkileriyle çek
        var reminders = await _db.Reminders
            .Include(r => r.Visit)!.ThenInclude(v => v!.Pet)!.ThenInclude(p => p!.Owner)
            .ToListAsync();

        var resp = new ReminderDashboardResponse
        {
            Today = reminders
                .Where(r => !r.IsCompleted && r.DueDate == today)
                .Select(MapToDashboardDto)
                .ToList(),

            Tomorrow = reminders
                .Where(r => !r.IsCompleted && r.DueDate == tomorrow)
                .Select(MapToDashboardDto)
                .ToList(),

            Overdue = reminders
                .Where(r => !r.IsCompleted && r.DueDate < today)
                .OrderBy(r => r.DueDate)
                .Select(MapToDashboardDto)
                .ToList(),

            Done = reminders
                .Where(r => r.IsCompleted)
                .OrderByDescending(r => r.CompletedAt)
                .Select(MapToDashboardDto)
                .ToList()
        };

        return Ok(resp);
    }

    // ============= ZİYARET DETAYI (modal için) =============
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
            PetId = v.PetId,                           // 🔴
            OwnerId = v.Pet.OwnerId,                   // 🔴
            PetName = v.Pet!.Name,
            OwnerName = v.Pet.Owner!.FullName,
            PerformedAt = v.PerformedAt,
            NextDate = v.NextDate.HasValue
                ? v.NextDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Zero))
                : (DateTime?)null,
            Purpose = v.Purpose,
            Procedures = v.Procedures ?? string.Empty,
            AmountTl = v.AmountTl,
            Notes = v.Notes ?? string.Empty,
            ImageUrl = v.ImageUrl
        };

        return Ok(dto);
    }
}
