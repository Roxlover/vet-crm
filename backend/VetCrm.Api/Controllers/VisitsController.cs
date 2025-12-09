using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Api.Services;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;
using VetCrm.Infrastructure.Storage;

namespace VetCrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly VetCrmDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IR2Storage _storage;

    public VisitsController(
        VetCrmDbContext db,
        ICurrentUserService currentUser,
        IR2Storage storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    /// <summary>
    /// Bir ziyaret için hem VisitPlan kayıtlarını hem de Reminders'ı senkronize eder.
    /// - plans doluysa: her satır için VisitPlan + Reminder üretir.
    /// - plans boşsa: legacy Visit.NextDate üzerinden 1 reminder üretir.
    /// </summary>
 private void SyncRemindersForVisit(Visit visit, List<VisitPlanCreateDto>? plans)
{
    var existingReminders = _db.Reminders.Where(r => r.VisitId == visit.Id);
    _db.Reminders.RemoveRange(existingReminders);

    // Yeni çoklu yapı: VisitPlans
    if (plans != null && plans.Count > 0)
    {
        foreach (var p in plans)
        {
            if (p == null || p.Date == default)
                continue;

            var dueDate = p.Date.AddDays(-1);

            // 1) Reminder
            var reminder = new Reminder
            {
                VisitId = visit.Id,
                DueDate = dueDate,
                Status  = ReminderStatus.Pending
            };
            _db.Reminders.Add(reminder);

            // 2) Appointment (takvim için)
            _db.Appointments.Add(new Appointment
            {
                VisitId     = visit.Id,
                OwnerId     = visit.Pet.OwnerId,
                PetId       = visit.PetId,
                ScheduledAt = p.Date.ToDateTime(new TimeOnly(10, 30)), // şimdilik sabit saat
                Purpose     = p.Purpose,
                DoctorId    = p.DoctorId
            });
        }

        return;
    }

    // Eski tekli NextDate davranışı (geriye uyum için)
    if (visit.NextDate is null)
        return;

    var legacyDue = visit.NextDate.Value.AddDays(-1);

    var legacyReminder = new Reminder
    {
        VisitId = visit.Id,
        DueDate = legacyDue,
        Status  = ReminderStatus.Pending
    };
    _db.Reminders.Add(legacyReminder);

    _db.Appointments.Add(new Appointment
    {
        VisitId     = visit.Id,
        OwnerId     = visit.Pet.OwnerId,
        PetId       = visit.PetId,
        ScheduledAt = visit.NextDate.Value.ToDateTime(new TimeOnly(10, 30)),
        Purpose     = visit.Purpose,
        DoctorId    = visit.DoctorId
    });
}

    // --------------------------------------------------------------------
    // GET /api/visits?ownerId=&petId=
    // --------------------------------------------------------------------
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VisitDto>>> GetVisits(
        [FromQuery] int? ownerId,
        [FromQuery] int? petId)
    {
        var query = _db.Visits
            .Include(v => v.Pet)
                .ThenInclude(p => p.Owner)
            .Include(v => v.CreatedByUser)
            .Include(v => v.Doctor)
            .Include(v => v.Images)
            .Include(v => v.Plans)      // 🔹 planları da çek
                .ThenInclude(p => p.Doctor)
            .AsQueryable();

        if (ownerId.HasValue)
            query = query.Where(v => v.Pet.OwnerId == ownerId.Value);

        if (petId.HasValue)
            query = query.Where(v => v.PetId == petId.Value);

        var visits = await query
            .OrderByDescending(v => v.PerformedAt)
            .Select(v => new VisitDto
            {
                Id = v.Id,
                PetId = v.PetId,
                PetName = v.Pet.Name,
                OwnerId = v.Pet.OwnerId,
                OwnerName = v.Pet.Owner.FullName,
                PerformedAt = v.PerformedAt,
                Procedures = v.Procedures,
                AmountTl = v.AmountTl,
                Notes = v.Notes,
                NextDate = v.NextDate,
                Purpose = v.Purpose,
                DoctorId = v.DoctorId,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : null,
                CreatedByUserId = v.CreatedByUserId,
                CreatedByUsername = v.CreatedByUsername,
                CreatedByName = v.CreatedByName,
                ImageUrl = v.ImageUrl,
                MicrochipNumber = v.MicrochipNumber,

                Images = v.Images
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new VisitImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        CreatedAt = i.CreatedAt
                    })
                    .ToList(),

                Plans = v.Plans
                    .OrderBy(p => p.Date)
                    .Select(p => new VisitPlanDto
                    {
                        Id = p.Id,
                        Date = p.Date,
                        Purpose = p.Purpose,
                        DoctorId = p.DoctorId,
                        DoctorName = p.Doctor != null ? p.Doctor.FullName : null
                    })
                    .ToList()
            })
            .ToListAsync();

        foreach (var dto in visits)
        {
            if (dto.ImageUrl == null)
                dto.ImageUrl = dto.Images.FirstOrDefault()?.ImageUrl;
        }

        return Ok(visits);
    }

    // --------------------------------------------------------------------
    // GET /api/visits/{id}
    // --------------------------------------------------------------------
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VisitDto>> GetVisit(int id)
    {
        var dto = await _db.Visits
            .Include(v => v.Pet)
                .ThenInclude(p => p.Owner)
            .Include(v => v.CreatedByUser)
            .Include(v => v.Doctor)
            .Include(v => v.Images)
            .Include(v => v.Plans)
                .ThenInclude(p => p.Doctor)
            .Where(v => v.Id == id)
            .Select(v => new VisitDto
            {
                Id = v.Id,
                PetId = v.PetId,
                PetName = v.Pet.Name,
                OwnerId = v.Pet.OwnerId,
                OwnerName = v.Pet.Owner.FullName,
                PerformedAt = v.PerformedAt,
                Procedures = v.Procedures,
                AmountTl = v.AmountTl,
                Notes = v.Notes,
                NextDate = v.NextDate,
                Purpose = v.Purpose,
                CreatedByUserId = v.CreatedByUserId,
                CreatedByUsername = v.CreatedByUsername,
                CreatedByName = v.CreatedByName,
                DoctorId = v.DoctorId,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : null,
                ImageUrl = v.ImageUrl,
                MicrochipNumber = v.MicrochipNumber,

                Images = v.Images
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new VisitImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        CreatedAt = i.CreatedAt
                    })
                    .ToList(),

                Plans = v.Plans
                    .OrderBy(p => p.Date)
                    .Select(p => new VisitPlanDto
                    {
                        Id = p.Id,
                        Date = p.Date,
                        Purpose = p.Purpose,
                        DoctorId = p.DoctorId,
                        DoctorName = p.Doctor != null ? p.Doctor.FullName : null
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (dto is null)
            return NotFound();

        if (dto.ImageUrl == null)
            dto.ImageUrl = dto.Images.FirstOrDefault()?.ImageUrl;

        return Ok(dto);
    }

    // --------------------------------------------------------------------
    // POST /api/visits
    // --------------------------------------------------------------------
    [HttpPost]
    public async Task<ActionResult<VisitDto>> CreateVisit([FromBody] VisitCreateDto dto)
    {
        Console.WriteLine("===== CreateVisit CALLED =====");

        try
        {
            Console.WriteLine("DTO: " + JsonSerializer.Serialize(dto));

            if (dto == null)
                return BadRequest("Body boş.");

            if (dto.PetId <= 0)
                return BadRequest("Geçersiz PetId.");

            var pet = await _db.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == dto.PetId);

            if (pet is null)
                return BadRequest($"Pet with id {dto.PetId} not found.");

            DateOnly? primaryNextDate = null;
            if (dto.NextVisits != null && dto.NextVisits.Count > 0)
            {
                primaryNextDate = dto.NextVisits
                    .Where(n => n != null && n.Date != default)
                    .Select(n => n.Date)
                    .OrderBy(d => d)
                    .FirstOrDefault();
            }


            var visit = new Visit
            {
                PetId = dto.PetId,
                PerformedAt = dto.PerformedAt ?? DateTime.UtcNow,
                Procedures = dto.Procedures,
                AmountTl = dto.AmountTl,
                Notes = dto.Notes,

                // Çoklu plan varsa en erken, yoksa legacy NextDate
                NextDate = primaryNextDate ?? dto.NextDate,
                Purpose = dto.Purpose,

                CreatedByUserId = _currentUser.UserId,
                CreatedByUsername = _currentUser.Username,
                CreatedByName = _currentUser.FullName,
                MicrochipNumber = dto.MicrochipNumber
            };

            var userId = _currentUser.UserId;
            if (userId.HasValue)
                visit.CreatedByUserId = userId.Value;

            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Plan + Reminders senkron
            SyncRemindersForVisit(visit, dto.NextVisits);            
            await _db.SaveChangesAsync();

            var result = new VisitDto
            {
                Id = visit.Id,
                PetId = visit.PetId,
                PetName = pet.Name,
                OwnerId = pet.OwnerId,
                OwnerName = pet.Owner.FullName,
                PerformedAt = visit.PerformedAt,
                Procedures = visit.Procedures,
                AmountTl = visit.AmountTl,
                Notes = visit.Notes,
                NextDate = visit.NextDate,
                Purpose = visit.Purpose,
                DoctorName = visit.Doctor != null ? visit.Doctor.FullName : null,
                CreatedByUserId = visit.CreatedByUserId,
                CreatedByUsername = visit.CreatedByUsername,
                CreatedByName = visit.CreatedByName,
                ImageUrl = visit.ImageUrl,
                MicrochipNumber = visit.MicrochipNumber,
                Plans = new()   // henüz mapper ile tekrar çekmiyoruz, istersen bırakabilirsin
            };

            Console.WriteLine("===== CreateVisit SUCCESS =====");
            return CreatedAtAction(nameof(GetVisit), new { id = visit.Id }, result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("===== CreateVisit ERROR =====");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, "CreateVisit ERROR: " + ex.Message);
        }
    }

    // --------------------------------------------------------------------
    // PUT /api/visits/{id}
    // --------------------------------------------------------------------
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVisit(int id, [FromBody] VisitUpdateDto dto)
    {
        var visit = await _db.Visits.FindAsync(id);
        if (visit is null)
            return NotFound();

        visit.PerformedAt = dto.PerformedAt;
        visit.Procedures = dto.Procedures;
        visit.AmountTl = dto.AmountTl;
        visit.Notes = dto.Notes;
        visit.MicrochipNumber = dto.MicrochipNumber;

        DateOnly? primaryNextDate = null;
        if (dto.NextVisits != null && dto.NextVisits.Count > 0)
        {
            primaryNextDate = dto.NextVisits
                .Where(n => n != null && n.Date != default)
                .Select(n => n.Date)
                .OrderBy(d => d)
                .FirstOrDefault();
        }


        visit.NextDate = primaryNextDate ?? dto.NextDate;
        visit.Purpose = dto.Purpose;

        SyncRemindersForVisit(visit, dto.Plans);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // --------------------------------------------------------------------
    // DELETE /api/visits/{id}
    // --------------------------------------------------------------------
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVisit(int id)
    {
        var visit = await _db.Visits.FindAsync(id);
        if (visit is null)
            return NotFound();

        var rems = _db.Reminders.Where(r => r.VisitId == id);
        _db.Reminders.RemoveRange(rems);

        var plans = _db.VisitPlans.Where(p => p.VisitId == id);
        _db.VisitPlans.RemoveRange(plans);

        _db.Visits.Remove(visit);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // --------------------------------------------------------------------
    // GET /api/visits/upcoming
    // (Şimdilik legacy NextDate üstünden gidiyor; ileride VisitPlans'e de genişletiriz)
    // --------------------------------------------------------------------
    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<UpcomingVisitDto>>> GetUpcoming([FromQuery] int days = 1)
    {
        var today = DateOnly.FromDateTime(DateTime.Now.Date);
        var target = today.AddDays(days);

        var upcoming = await _db.Visits
            .Include(v => v.Pet)
                .ThenInclude(p => p.Owner)
            .Where(v => v.NextDate == target)
            .Select(v => new UpcomingVisitDto
            {
                VisitId = v.Id,
                PetId = v.PetId,
                PetName = v.Pet.Name,
                OwnerId = v.Pet.OwnerId,
                OwnerName = v.Pet.Owner.FullName,
                OwnerPhoneE164 = v.Pet.Owner.PhoneE164,
                VisitDate = v.NextDate!.Value,
                Procedures = v.Procedures,
                WhatsAppSent = false
            })
            .OrderBy(u => u.OwnerName)
            .ThenBy(u => u.PetName)
            .ToListAsync();

        return Ok(upcoming);
    }

    // --------------------------------------------------------------------
    // ÇOKLU GÖRSEL UPLOAD
    // --------------------------------------------------------------------
    [HttpPost("{id:int}/images")]
    public async Task<ActionResult<List<VisitImageDto>>> UploadImages(
        int id,
        [FromForm] List<IFormFile> files)
    {
        var visit = await _db.Visits.FindAsync(id);
        if (visit is null)
            return NotFound();

        if (files == null || files.Count == 0)
            return BadRequest("Dosya yok.");

        var results = new List<VisitImageDto>();

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var url = await _storage.UploadVisitImageAsync(
                visitId: visit.Id,
                stream: stream,
                contentType: file.ContentType ?? "image/jpeg"
            );

            var image = new VisitImage
            {
                VisitId = visit.Id,
                ImageUrl = url,
                CreatedAt = DateTime.UtcNow
            };

            _db.VisitImages.Add(image);

            results.Add(new VisitImageDto
            {
                Id = image.Id,
                ImageUrl = url,
                CreatedAt = image.CreatedAt
            });
        }

        await _db.SaveChangesAsync();
        return Ok(results);
    }
}
