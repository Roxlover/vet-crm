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
public async Task<ActionResult<List<ReminderItemDto>>> GetReminders(
    [FromQuery] string filter = "upcoming")
{
    var today = DateOnly.FromDateTime(DateTime.Today);
    var tomorrow = today.AddDays(1);

    IQueryable<Reminder> query = _db.Reminders.AsQueryable();

    switch (filter.ToLowerInvariant())
    {
        case "today":
            query = query.Where(r => r.DueDate == today && !r.IsCompleted);
            break;

        case "tomorrow":
            query = query.Where(r => r.DueDate == tomorrow && !r.IsCompleted);
            break;

        case "overdue":
            query = query.Where(r => r.DueDate < today && !r.IsCompleted);
            break;

        case "done":
            query = query.Where(r => r.IsCompleted)
                         .OrderByDescending(r => r.CompletedAt);
            break;

        default: 

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
            Procedures = r.Visit!.Procedures ?? string.Empty,
            CreditAmountTl = r.Visit!.CreditAmountTl 
        })
        .ToListAsync();

    return Ok(items);
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
            CreatedByName     = v.CreatedByName
        })
        .FirstOrDefaultAsync();

    if (dto == null) return NotFound();
    return Ok(dto);
}

}
