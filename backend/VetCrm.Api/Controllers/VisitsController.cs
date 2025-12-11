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

    private void SyncRemindersForVisit(Visit visit, List<VisitPlanCreateDto>? plans)
    {
        var oldReminders    = _db.Reminders   .Where(r => r.VisitId == visit.Id);
        var oldPlans        = _db.VisitPlans  .Where(p => p.VisitId == visit.Id);
        var oldAppointments = _db.Appointments.Where(a => a.VisitId == visit.Id);

        _db.Reminders.RemoveRange(oldReminders);
        _db.VisitPlans.RemoveRange(oldPlans);
        _db.Appointments.RemoveRange(oldAppointments);

        if (plans == null || plans.Count == 0)
        {
            if (visit.NextDate is null)
                return;

            var next = visit.NextDate.Value;
            var due  = next.AddDays(-1);

            _db.Reminders.Add(new Reminder
            {
                VisitId = visit.Id,
                DueDate = due,
                Status  = ReminderStatus.Pending
            });

            _db.Appointments.Add(new Appointment
            {
                VisitId     = visit.Id,
                OwnerId     = visit.Pet.OwnerId,
                PetId       = visit.PetId,
                ScheduledAt = next.ToDateTime(new TimeOnly(10, 30)),
                Purpose     = visit.Purpose,
                DoctorId    = visit.DoctorId,
            });

            return;
        }

        foreach (var p in plans)
        {
            if (p == null || p.Date == default)
                continue;

            var vp = new VisitPlan
            {
                VisitId  = visit.Id,
                Date     = p.Date,
                Purpose  = p.Purpose,
                DoctorId = p.DoctorId,
            };
            _db.VisitPlans.Add(vp);

            var due = p.Date.AddDays(-1);
            _db.Reminders.Add(new Reminder
            {
                VisitId = visit.Id,
                DueDate = due,
                Status  = ReminderStatus.Pending
            });

            _db.Appointments.Add(new Appointment
            {
                VisitId     = visit.Id,
                OwnerId     = visit.Pet.OwnerId,
                PetId       = visit.PetId,
                ScheduledAt = p.Date.ToDateTime(new TimeOnly(10, 30)),
                Purpose     = p.Purpose,
                DoctorId    = p.DoctorId,
            });
        }
    }

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
            .Include(v => v.Plans)
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
            if (dto.Plans != null && dto.Plans.Count > 0)
            {
                primaryNextDate = dto.Plans
                    .Where(p => p != null && p.Date != default)
                    .Select(p => p.Date)
                    .OrderBy(d => d)
                    .FirstOrDefault();
            }

            var visit = new Visit
            {
                PetId       = dto.PetId,
                PerformedAt = dto.PerformedAt ?? DateTime.UtcNow,
                Procedures  = dto.Procedures,
                AmountTl    = dto.AmountTl,
                Notes       = dto.Notes,
                NextDate    = primaryNextDate ?? dto.NextDate,
                Purpose     = dto.Purpose,
                CreatedByUserId   = _currentUser.UserId,
                CreatedByUsername = _currentUser.Username,
                CreatedByName     = _currentUser.FullName,
                MicrochipNumber   = dto.MicrochipNumber
            };

            var userId = _currentUser.UserId;
            if (userId.HasValue)
                visit.CreatedByUserId = userId.Value;

            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();
            _db.Entry(visit).Reference(v => v.Pet).Load();

            SyncRemindersForVisit(visit, dto.Plans);
            await _db.SaveChangesAsync();

            _db.Entry(visit).Reference(v => v.Pet).Load();
            _db.Entry(visit.Pet).Reference(p => p.Owner).Load();

            SyncRemindersForVisit(visit, dto.Plans);
            await _db.SaveChangesAsync();

            var result = new VisitDto
            {
                Id          = visit.Id,
                PetId       = visit.PetId,
                PetName     = pet.Name,
                OwnerId     = pet.OwnerId,
                OwnerName   = pet.Owner.FullName,
                PerformedAt = visit.PerformedAt,
                Procedures  = visit.Procedures,
                AmountTl    = visit.AmountTl,
                Notes       = visit.Notes,
                NextDate    = visit.NextDate,
                Purpose     = visit.Purpose,
                DoctorName  = visit.Doctor != null ? visit.Doctor.FullName : null,
                CreatedByUserId   = visit.CreatedByUserId,
                CreatedByUsername = visit.CreatedByUsername,
                CreatedByName     = visit.CreatedByName,
                ImageUrl          = visit.ImageUrl,
                MicrochipNumber   = visit.MicrochipNumber,
                Plans             = new()
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVisit(int id, [FromBody] VisitUpdateDto dto)
    {
        var visit = await _db.Visits
            .Include(v => v.Pet) 
            .FirstOrDefaultAsync(v => v.Id == id);

        if (visit is null)
            return NotFound();

        visit.PerformedAt     = dto.PerformedAt;
        visit.Procedures      = dto.Procedures;
        visit.AmountTl        = dto.AmountTl;
        visit.Notes           = dto.Notes;
        visit.MicrochipNumber = dto.MicrochipNumber;

        DateOnly? primaryNextDate = null;
        if (dto.Plans != null && dto.Plans.Count > 0)
        {
            primaryNextDate = dto.Plans
                .Where(p => p != null && p.Date != default)
                .Select(p => p.Date)
                .OrderBy(d => d)
                .FirstOrDefault();
        }

        visit.NextDate = primaryNextDate ?? dto.NextDate;
        visit.Purpose  = dto.Purpose;


        SyncRemindersForVisit(visit, dto.Plans);

        await _db.SaveChangesAsync();

        return NoContent();
    }

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

        var apps = _db.Appointments.Where(a => a.VisitId == id);
        _db.Appointments.RemoveRange(apps);

        _db.Visits.Remove(visit);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<UpcomingVisitDto>>> GetUpcoming([FromQuery] int days = 1)
    {
        var today  = DateOnly.FromDateTime(DateTime.Now.Date);
        var target = today.AddDays(days);

        var upcoming = await _db.Visits
            .Include(v => v.Pet)
                .ThenInclude(p => p.Owner)
            .Where(v => v.NextDate == target)
            .Select(v => new UpcomingVisitDto
            {
                VisitId        = v.Id,
                PetId          = v.PetId,
                PetName        = v.Pet.Name,
                OwnerId        = v.Pet.OwnerId,
                OwnerName      = v.Pet.Owner.FullName,
                OwnerPhoneE164 = v.Pet.Owner.PhoneE164,
                VisitDate      = v.NextDate!.Value,
                Procedures     = v.Procedures,
                WhatsAppSent   = false
            })
            .OrderBy(u => u.OwnerName)
            .ThenBy(u => u.PetName)
            .ToListAsync();

        return Ok(upcoming);
    }

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
                VisitId  = visit.Id,
                ImageUrl = url,
                CreatedAt = DateTime.UtcNow
            };

            _db.VisitImages.Add(image);

            results.Add(new VisitImageDto
            {
                Id       = image.Id,
                ImageUrl = url,
                CreatedAt = image.CreatedAt
            });
        }

        await _db.SaveChangesAsync();
        return Ok(results);
    }
}
