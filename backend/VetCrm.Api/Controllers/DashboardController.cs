using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Infrastructure.Data;
using VetCrm.Domain.Entities;

namespace VetCrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public DashboardController(VetCrmDbContext db)
    {
        _db = db;
    }

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
            IsCompleted = r.Visit!.Status == Visit.VisitStatus.Completed,
            VisitImageUrl = r.Visit!.ImageUrl,
            VisitStatus = r.Visit!.Status.ToString()

        };
    }

   [HttpGet("reminders-summary")]
public async Task<ActionResult<ReminderSummaryDto>> GetRemindersSummary()
{
    var today = DateOnly.FromDateTime(DateTime.Today);
    var tomorrow = today.AddDays(1);

    // Bugün: yapılmadı + due bugün
    var pendingToday = await _db.Reminders
        .Where(r => r.IsCompleted == false && r.DueDate == today)
        .CountAsync();

    // Yarın: yapılmadı + due yarın
    var pendingTomorrow = await _db.Reminders
        .Where(r => r.IsCompleted == false && r.DueDate == tomorrow)
        .CountAsync();

    // Geciken: yapılmadı + due bugün'den küçük
    var overdue = await _db.Reminders
        .Where(r => r.IsCompleted == false && r.DueDate < today)
        .CountAsync();

    // Yapıldı: IsCompleted = true
    var completed = await _db.Reminders
        .Where(r => r.IsCompleted == true)
        .CountAsync();

    // Upcoming listesi: yapılmadı + due bugünden büyük
    var upcoming = await _db.Reminders
        .Include(r => r.Visit)!.ThenInclude(v => v!.Pet)!.ThenInclude(p => p!.Owner)
        .Where(r =>
            r.IsCompleted == false &&
            r.Visit != null &&
            r.DueDate > today
        )
        .OrderBy(r => r.DueDate)
        .Take(5)
        .Select(r => new ReminderItemDto
        {
            Id = r.Id,
            VisitId = r.VisitId,
            ReminderDate = r.DueDate,
            AppointmentDate = r.Visit!.NextDate ?? r.DueDate,
            PetName = r.Visit!.Pet!.Name,
            OwnerName = r.Visit!.Pet!.Owner!.FullName,
            Procedures = r.Visit!.Procedures ?? string.Empty,
            CreditAmountTl = r.Visit!.CreditAmountTl
        })
        .ToListAsync();

    var dto = new ReminderSummaryDto
    {
        PendingToday = pendingToday,
        PendingTomorrow = pendingTomorrow,
        Overdue = overdue,
        Completed = completed,
        Upcoming = upcoming
    };

    return Ok(dto);
}

    [HttpGet("reminders")]
    public async Task<IActionResult> GetReminders([FromQuery] string filter = "upcoming")
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var tomorrow = today.AddDays(1);

        // DB'den DateOnly üretmek yerine DateTime? çekip memory'de DateOnly'e çeviriyoruz.
        var baseQuery =
            from r in _db.Reminders
            join v in _db.Visits on r.VisitId equals v.Id
            join pet in _db.Pets on v.PetId equals pet.Id
            join owner in _db.Owners on pet.OwnerId equals owner.Id
            select new
            {
                id = r.Id,
                visitId = v.Id,
                reminderDate = r.DueDate,

                appointmentScheduledAt = _db.Appointments
                    .Where(a => a.VisitId == v.Id)
                    .OrderBy(a => a.ScheduledAt)
                    .Select(a => (DateTime?)a.ScheduledAt)
                    .FirstOrDefault(),

                petName = pet.Name,
                ownerName = owner.FullName,
                procedures = v.Procedures,
                creditAmountTl = v.CreditAmountTl,
                visitStatus = v.Status,
                r.Status,
                r.IsCompleted
            };

switch (filter)
{
    case "today":
        baseQuery = baseQuery.Where(x =>
            !x.IsCompleted &&
            x.reminderDate == today
        );
        break;

    case "tomorrow":
        baseQuery = baseQuery.Where(x =>
            !x.IsCompleted &&
            x.reminderDate == tomorrow
        );
        break;

    case "overdue":
        baseQuery = baseQuery.Where(x =>
            !x.IsCompleted &&
            x.reminderDate < today
        );
        break;

    case "done":
        baseQuery = baseQuery.Where(x =>
            x.IsCompleted
        );
        break;

    default: // upcoming
        baseQuery = baseQuery.Where(x =>
            !x.IsCompleted &&
            x.reminderDate >= today
        );
        break;
}

        var rows = await baseQuery
            .OrderBy(x => x.reminderDate)
            .ThenBy(x => x.appointmentScheduledAt)
            .ToListAsync();
        rows = rows
    .GroupBy(x => x.id)
    .Select(g => g.First())
    .ToList();


        var list = rows.Select(x => new
{
    x.id,
    x.visitId,
    reminderDate = x.reminderDate,
    appointmentDate = x.appointmentScheduledAt.HasValue
        ? DateOnly.FromDateTime(x.appointmentScheduledAt.Value)
        : (DateOnly?)null,
    x.petName,
    x.ownerName,
    procedures = x.procedures,
    x.creditAmountTl,

    // yeni alan
    visitStatus = x.visitStatus.ToString()
}).ToList();

        return Ok(list);
    }

    [HttpGet("reminders-dashboard")]
    public async Task<ActionResult<ReminderDashboardResponse>> GetRemindersDashboard()
    {
        // HATA 1 FIX: today önce tanımlanır
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tomorrow = today.AddDays(1);

        // HATA 2 FIX: DateDiffDay yok (SQL Server). Npgsql için kaldırıyoruz.
        // Duplicate FIX: Join yerine tek appointment seçiyoruz (en erken).
        var reminders = await _db.Reminders
            .Where(r => r.DueDate >= today)
            .OrderBy(r => r.DueDate)
            .Include(r => r.Visit)!.ThenInclude(v => v!.Pet)!.ThenInclude(p => p!.Owner)
            .Select(r => new
            {
                Reminder = r,
                AppointmentScheduledAt = _db.Appointments
                    .Where(a => a.VisitId == r.VisitId)
                    .OrderBy(a => a.ScheduledAt)
                    .Select(a => (DateTime?)a.ScheduledAt)
                    .FirstOrDefault()
            })
            .ToListAsync();
        reminders = reminders
    .GroupBy(x => x.Reminder.Id)
    .Select(g => g.First())
    .ToList();

        // DashboardResponse senin mevcut MapToDashboardDto(Reminder) yapını koruyor.
        // Appointment tarihi gerekiyorsa DTO'ya yeni alan ekleyip burada set ederiz.
        var resp = new ReminderDashboardResponse
{
    Today = reminders
        .Where(x =>
            x.Reminder.Visit != null &&
            x.Reminder.Visit.Status == Visit.VisitStatus.Pending &&
            x.Reminder.DueDate == today
        )
        .Select(x => MapToDashboardDto(x.Reminder))
        .ToList(),

    Tomorrow = reminders
        .Where(x =>
            x.Reminder.Visit != null &&
            x.Reminder.Visit.Status == Visit.VisitStatus.Pending &&
            x.Reminder.DueDate == tomorrow
        )
        .Select(x => MapToDashboardDto(x.Reminder))
        .ToList(),

    Overdue = reminders
        .Where(x =>
            x.Reminder.Visit != null &&
            (
                x.Reminder.Visit.Status == Visit.VisitStatus.Missed ||
                (x.Reminder.Visit.Status == Visit.VisitStatus.Pending && x.Reminder.DueDate < today)
            )
        )
        .OrderBy(x => x.Reminder.DueDate)
        .Select(x => MapToDashboardDto(x.Reminder))
        .ToList(),

    Done = reminders
        .Where(x =>
            x.Reminder.Visit != null &&
            x.Reminder.Visit.Status == Visit.VisitStatus.Completed
        )
        .OrderByDescending(x => x.Reminder.Visit!.StatusUpdatedAt ?? x.Reminder.CompletedAt)
        .Select(x => MapToDashboardDto(x.Reminder))
        .ToList()
};

        return Ok(resp);
    }

    [HttpGet("visit/{id:int}")]
    public async Task<ActionResult<DashboardVisitDetailDto>> GetVisitDetail(int id)
    {
var dto = await _db.Visits
    .Where(v => v.Id == id)
    .Select(v => new DashboardVisitDetailDto
    {
        Id = v.Id,
        PetId = v.PetId,
        PetName = v.Pet.Name,
        OwnerId = v.Pet.OwnerId,
        OwnerName = v.Pet.Owner.FullName,
        PerformedAt = v.PerformedAt,
        NextDate = v.NextDate,
        Purpose = v.Purpose,
        Procedures = v.Procedures,
        AmountTl = v.AmountTl,
        Notes = v.Notes,
        CreditAmountTl = v.CreditAmountTl,
        ImageUrl = v.ImageUrl,

        DoctorId = v.DoctorId,
        DoctorName = v.Doctor != null ? v.Doctor.FullName : null,

        CreatedByUserId = v.CreatedByUserId,
        CreatedByUsername = v.CreatedByUsername,
        CreatedByName = v.CreatedByName,
        MicrochipNumber = v.MicrochipNumber,

        Images = _db.VisitImages
            .Where(img => img.VisitId == v.Id)
            .OrderByDescending(img => img.CreatedAt)
            .Select(img => new VisitImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                CreatedAt = img.CreatedAt
            })
            .ToList()
    })
    .FirstOrDefaultAsync();

        if (dto == null) return NotFound();

        dto.NextVisits = await _db.Appointments
            .Where(a => a.VisitId == id)
            .OrderBy(a => a.ScheduledAt)
            .Select(a => new NextVisitItemDto
            {
                Id = a.Id,
                NextDate = a.ScheduledAt,
                Purpose = a.Purpose
            })
            .ToListAsync();

        return Ok(dto);
    }
}
