using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace VetCrm.Api.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public CalendarController(VetCrmDbContext db)
    {
        _db = db;
    }

    public class CalendarAppointmentDto
    {
        public int VisitId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string PetName { get; set; } = null!;
        public string OwnerName { get; set; } = null!;
        public string? Purpose { get; set; }
        public string? DoctorName { get; set; }

        public string? CreatedByName { get; set; }
        public string? CreatedByUsername { get; set; }
        public decimal? CreditAmountTl { get; set; }
    }

    // /api/calendar/appointments?from=2025-11-01&to=2025-11-30
    [HttpGet("appointments")]
    public async Task<ActionResult<List<CalendarAppointmentDto>>> GetAppointments(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        var visits = await _db.Visits
            .Include(v => v.Pet)!.ThenInclude(p => p.Owner)
            .Include(v => v.Doctor)
            .Include(v => v.CreatedByUser)
            .Where(v => v.NextDate != null &&
                        v.NextDate >= from &&
                        v.NextDate <= to)
            .OrderBy(v => v.NextDate)
            .ThenBy(v => v.PerformedAt)
            .ToListAsync();

        var dtos = visits.Select(v => new CalendarAppointmentDto
        {
            VisitId = v.Id,
            ScheduledAt = v.PerformedAt,
            PetName = v.Pet!.Name,
            OwnerName = v.Pet.Owner!.FullName,
            Purpose = v.Purpose,
            DoctorName = v.Doctor != null ? v.Doctor.FullName : null,
            CreatedByUsername = v.CreatedByUser != null ? v.CreatedByUser.Username : null,
            CreatedByName = v.CreatedByUser != null ? v.CreatedByUser.FullName : null,
            CreditAmountTl = v.CreditAmountTl,

        }).ToList();

        return Ok(dtos);
    }
}
