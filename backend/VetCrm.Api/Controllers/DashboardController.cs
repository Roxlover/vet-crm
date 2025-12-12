using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Infrastructure.Data;
using VetCrm.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

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
            IsCompleted = r.IsCompleted,         
            VisitImageUrl = r.Visit!.ImageUrl     
        };
    }

[HttpGet("reminders-summary")]
public async Task<ActionResult<ReminderSummaryDto>> GetRemindersSummary()
{
    var today = DateOnly.FromDateTime(DateTime.Today);
    var tomorrow = today.AddDays(1);

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
    var today    = DateOnly.FromDateTime(DateTime.UtcNow.Date);
    var tomorrow = today.AddDays(1);

    var query =
        from r in _db.Reminders
        join v   in _db.Visits on r.VisitId equals v.Id
        join pet in _db.Pets   on v.PetId equals pet.Id
        join owner in _db.Owners on pet.OwnerId equals owner.Id
        join a in _db.Appointments on v.Id equals a.VisitId into apptJoin
        from a in apptJoin.DefaultIfEmpty()
        select new
        {
            id             = r.Id,
            visitId        = v.Id,
            reminderDate   = r.DueDate,
            appointmentDate = a != null 
                ? DateOnly.FromDateTime(a.ScheduledAt) 
                : (DateOnly?)null,
            petName        = pet.Name,
            ownerName      = owner.FullName,
            procedures     = v.Procedures,
            creditAmountTl = v.CreditAmountTl,
            r.Status,
            r.IsCompleted
        };

    switch (filter)
    {
        case "today":
            query = query.Where(x => x.reminderDate == today && !x.IsCompleted);
            break;
        case "tomorrow":
            query = query.Where(x => x.reminderDate == tomorrow && !x.IsCompleted);
            break;
        case "overdue":
            query = query.Where(x => x.reminderDate < today && !x.IsCompleted);
            break;
        case "done":
            query = query.Where(x => x.IsCompleted);
            break;
        default:
            query = query.Where(x => x.reminderDate >= today && !x.IsCompleted);
            break;
    }

    var list = await query
        .OrderBy(x => x.reminderDate)
        .ThenBy(x => x.appointmentDate)
        .ToListAsync();

    return Ok(list);
}

    [HttpGet("reminders-dashboard")]
    public async Task<ActionResult<ReminderDashboardResponse>> GetRemindersDashboard()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

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

  [HttpGet("visit/{id:int}")]
public async Task<ActionResult<DashboardVisitDetailDto>> GetVisitDetail(int id)
{
    var dto = await _db.Visits
        .Include(v => v.Pet).ThenInclude(p => p.Owner)
        .Include(v => v.Doctor)
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
            ImageUrl = v.ImageUrl ,

            DoctorId = v.DoctorId,
            DoctorName = v.Doctor != null ? v.Doctor.FullName : null,

            CreatedByUserId   = v.CreatedByUserId,
            CreatedByUsername = v.CreatedByUsername,
            CreatedByName     = v.CreatedByName,

            MicrochipNumber   = v.MicrochipNumber
        })
        .FirstOrDefaultAsync();

    if (dto == null) return NotFound();

    // BURAYA EKLE: Appointment'lardan nextVisits listesi
    dto.NextVisits = await _db.Appointments
        .Where(a => a.VisitId == id)
        .OrderBy(a => a.ScheduledAt)
        .Select(a => new NextVisitItemDto
        {
            Id       = a.Id,
            NextDate = a.ScheduledAt,
            Purpose  = a.Purpose
        })
        .ToListAsync();

    return Ok(dto);
}


}
