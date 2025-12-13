using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Infrastructure.Data;

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
        public int Id { get; set; }              // Appointment Id
        public int? VisitId { get; set; }        // Bağlı olduğu ziyaret
        public DateTime ScheduledAt { get; set; }

        public string PetName { get; set; } = null!;
        public string OwnerName { get; set; } = null!;
        public string? Purpose { get; set; }
        public string? DoctorName { get; set; }

        public string? CreatedByName { get; set; }
        public string? CreatedByUsername { get; set; }
        public decimal? CreditAmountTl { get; set; }
    }

[HttpGet("appointments")]
public async Task<ActionResult<List<CalendarAppointmentDto>>> GetAppointments(
    [FromQuery] DateOnly from,
    [FromQuery] DateOnly to)
{
    TimeZoneInfo tz;
    try
    {
        tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
    }
    catch
    {
        tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
    }

    var fromLocalUnspec = from.ToDateTime(TimeOnly.MinValue);
    var toLocalUnspec   = to.ToDateTime(TimeOnly.MaxValue);

    fromLocalUnspec = DateTime.SpecifyKind(fromLocalUnspec, DateTimeKind.Unspecified);
    toLocalUnspec   = DateTime.SpecifyKind(toLocalUnspec, DateTimeKind.Unspecified);

    var fromUtc = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(fromLocalUnspec, tz), DateTimeKind.Utc);
    var toUtc   = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(toLocalUnspec, tz), DateTimeKind.Utc);

    var list = await (
        from a in _db.Appointments
        join v in _db.Visits on a.VisitId equals v.Id
        join pet in _db.Pets on a.PetId equals pet.Id
        join owner in _db.Owners on pet.OwnerId equals owner.Id
        join doc in _db.Users on a.DoctorId equals doc.Id into docJoin
        from doc in docJoin.DefaultIfEmpty()
        join creator in _db.Users on v.CreatedByUserId equals creator.Id into creatorJoin
        from creator in creatorJoin.DefaultIfEmpty()
        where a.ScheduledAt >= fromUtc && a.ScheduledAt <= toUtc
        orderby a.ScheduledAt, owner.FullName, pet.Name
        select new CalendarAppointmentDto
        {
            Id = a.Id,
            VisitId = v.Id,
            ScheduledAt = a.ScheduledAt,
            PetName = pet.Name,
            OwnerName = owner.FullName,
            Purpose = a.Purpose,
            DoctorName = doc != null ? doc.FullName : null,
            CreatedByUsername = creator != null ? creator.Username : null,
            CreatedByName = creator != null ? creator.FullName : null,
            CreditAmountTl = v.CreditAmountTl
        }
    ).ToListAsync();

    return Ok(list);
}


}
