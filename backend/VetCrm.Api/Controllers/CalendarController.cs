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
        public int Id { get; set; }              // Record Id
        public int? VisitId { get; set; }        // Bağlı olduğu ziyaret (veya bizzat ziyaret id)
        public DateTime ScheduledAt { get; set; }
        public bool IsVisit { get; set; }        // Randevu mu yoksa gerçekleşmiş ziyaret mi?

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
    // Cross-platform TZ
    TimeZoneInfo tz;
    try
    {
        tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); // Windows
    }
    catch
    {
        tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); // Linux
    }

    // DateOnly -> local wall-clock (Unspecified)
    var fromLocal = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
    var toLocal   = DateTime.SpecifyKind(to.ToDateTime(TimeOnly.MaxValue),   DateTimeKind.Unspecified);

    // Convert to UTC and force Kind=Utc explicitly (Npgsql wants this for timestamptz)
    var fromUtc = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(fromLocal, tz), DateTimeKind.Utc);
    var toUtc   = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(toLocal, tz),   DateTimeKind.Utc);

    // 1) Randevuları Çek (Appointments)
    var appointments = await (
        from a in _db.Appointments
        join pet in _db.Pets on a.PetId equals pet.Id
        join owner in _db.Owners on pet.OwnerId equals owner.Id
        join v in _db.Visits on a.VisitId equals v.Id into vJoin
        from v in vJoin.DefaultIfEmpty()
        join doc in _db.Users on a.DoctorId equals doc.Id into docJoin
        from doc in docJoin.DefaultIfEmpty()
        join creator in _db.Users on v.CreatedByUserId equals creator.Id into creatorJoin
        from creator in creatorJoin.DefaultIfEmpty()
        where a.ScheduledAt >= fromUtc && a.ScheduledAt <= toUtc
        select new CalendarAppointmentDto
        {
            Id = a.Id,
            VisitId = a.VisitId,
            ScheduledAt = a.ScheduledAt,
            IsVisit = false,
            PetName = pet.Name,
            OwnerName = owner.FullName,
            Purpose = a.Purpose,
            DoctorName = doc != null ? doc.FullName : null,
            CreatedByUsername = creator != null ? creator.Username : null,
            CreatedByName = creator != null ? creator.FullName : null,
            CreditAmountTl = v != null ? v.CreditAmountTl : null
        }
    ).ToListAsync();

    // 2) Gerçekleşen Ziyaretleri Çek (Visits) - Randevusu olmayan veya bizzat geçmiş kayıtlar için
    var visits = await (
        from v in _db.Visits
        join pet in _db.Pets on v.PetId equals pet.Id
        join owner in _db.Owners on pet.OwnerId equals owner.Id
        join doc in _db.Users on v.DoctorId equals doc.Id into docJoin
        from doc in docJoin.DefaultIfEmpty()
        join creator in _db.Users on v.CreatedByUserId equals creator.Id into creatorJoin
        from creator in creatorJoin.DefaultIfEmpty()
        where v.PerformedAt >= fromUtc && v.PerformedAt <= toUtc
        select new CalendarAppointmentDto
        {
            Id = v.Id,
            VisitId = v.Id,
            ScheduledAt = v.PerformedAt,
            IsVisit = true,
            PetName = pet.Name,
            OwnerName = owner.FullName,
            Purpose = v.Purpose,
            DoctorName = doc != null ? doc.FullName : null,
            CreatedByUsername = creator != null ? creator.Username : null,
            CreatedByName = creator != null ? creator.FullName : null,
            CreditAmountTl = v.CreditAmountTl
        }
    ).ToListAsync();

    // 3) Birleştir ve Mükerrerleri (Aynı ziyarete bağlı randevu varsa ziyareti tercih et) Ayıkla
    var combined = appointments
        .Where(a => a.VisitId == null || !visits.Any(v => v.Id == a.VisitId))
        .Concat(visits)
        .OrderBy(x => x.ScheduledAt)
        .ToList();

    return Ok(combined);
}


}
