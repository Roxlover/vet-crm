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

    private void SyncReminderForVisit(Visit visit)
    {
        var existing = _db.Reminders.Where(r => r.VisitId == visit.Id);
        _db.Reminders.RemoveRange(existing);

        if (visit.NextDate is null)
            return;

        var next = visit.NextDate.Value;
        var due = next.AddDays(-1);

        var reminder = new Reminder
        {
            VisitId = visit.Id,
            DueDate = due,
            Status = ReminderStatus.Pending
        };

        _db.Reminders.Add(reminder);
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
                DoctorId = v.DoctorId,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : null,
                CreatedByUserId   = v.CreatedByUserId,
                CreatedByUsername = v.CreatedByUsername,
                CreatedByName     = v.CreatedByName,
                ImageUrl          = v.ImageUrl
            })
            .ToListAsync();

        return Ok(visits);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VisitDto>> GetVisit(int id)
    {
        var visit = await _db.Visits
            .Include(v => v.Pet)
                .ThenInclude(p => p.Owner)
            .Include(v => v.CreatedByUser)
            .Include(v => v.Doctor)
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
                CreatedByUserId   = v.CreatedByUserId,
                CreatedByUsername = v.CreatedByUsername,
                CreatedByName     = v.CreatedByName,
                ImageUrl          = v.ImageUrl
            })
            .FirstOrDefaultAsync();

        if (visit is null)
            return NotFound();

        return Ok(visit);
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

            var visit = new Visit
            {
                PetId       = dto.PetId,
                PerformedAt = dto.PerformedAt ?? DateTime.UtcNow,
                Procedures  = dto.Procedures,
                AmountTl    = dto.AmountTl,
                Notes       = dto.Notes,
                NextDate    = dto.NextDate,
                Purpose     = dto.Purpose,
                CreatedByUserId   = _currentUser.UserId,
                CreatedByUsername = _currentUser.Username,
                CreatedByName     = _currentUser.FullName
            };

            var userId = _currentUser.UserId;
            if (userId.HasValue)
                visit.CreatedByUserId = userId.Value;

            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            SyncReminderForVisit(visit);
            await _db.SaveChangesAsync();

            var result = new VisitDto
            {
                Id         = visit.Id,
                PetId      = visit.PetId,
                PetName    = pet.Name,
                OwnerId    = pet.OwnerId,
                OwnerName  = pet.Owner.FullName,
                PerformedAt = visit.PerformedAt,
                Procedures  = visit.Procedures,
                AmountTl    = visit.AmountTl,
                Notes       = visit.Notes,
                NextDate    = visit.NextDate,
                DoctorName  = visit.Doctor != null ? visit.Doctor.FullName : null,
                CreatedByUserId   = visit.CreatedByUserId,
                CreatedByUsername = visit.CreatedByUsername,
                CreatedByName     = visit.CreatedByName,
                ImageUrl          = visit.ImageUrl
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
        var visit = await _db.Visits.FindAsync(id);
        if (visit is null)
            return NotFound();

        visit.PerformedAt = dto.PerformedAt;
        visit.Procedures  = dto.Procedures;
        visit.AmountTl    = dto.AmountTl;
        visit.Notes       = dto.Notes;
        visit.NextDate    = dto.NextDate;
        visit.Purpose     = dto.Purpose;

        SyncReminderForVisit(visit);

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

    // ---------- LOCAL IMAGE UPLOAD ----------

    [HttpPost("{id:int}/image")]
    public async Task<ActionResult<VisitImageDto>> UploadImage(int id, IFormFile file)
    {
        Console.WriteLine("=== UploadImage CALLED === id=" + id);
        Console.WriteLine($"File? name={file?.FileName}, len={file?.Length}");

        var visit = await _db.Visits.FindAsync(id);
        if (visit is null)
            return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest("Dosya yok.");

        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadVisitImageAsync(
            visitId: visit.Id,
            stream: stream,
            contentType: file.ContentType ?? "image/jpeg"
        );

        visit.ImageUrl = url;
        await _db.SaveChangesAsync();

        Console.WriteLine("[UploadImage] SUCCESS, url=" + url);

        return Ok(new VisitImageDto { ImageUrl = url });
    }
}
