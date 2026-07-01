using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Api.Services;
using VetCrm.Api.Storage;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;
using System.Security.Claims;

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
    var oldReminders    = _db.Reminders.Where(r => r.VisitId == visit.Id);
    var oldPlans        = _db.VisitPlans.Where(p => p.VisitId == visit.Id);
    var oldAppointments = _db.Appointments.Where(a => a.VisitId == visit.Id);

    _db.Reminders.RemoveRange(oldReminders);
    _db.VisitPlans.RemoveRange(oldPlans);
    _db.Appointments.RemoveRange(oldAppointments);

    if (plans == null || plans.Count == 0)
    {
        if (visit.NextDate is null)
            return;

        var utc = IstanbulDateOnlyToUtc(visit.NextDate.Value);

        _db.Reminders.Add(new Reminder
        {
            VisitId = visit.Id,
            DueDate = visit.NextDate.Value.AddDays(-1),
            Status  = ReminderStatus.Pending
        });

        _db.Appointments.Add(new Appointment
        {
            VisitId     = visit.Id,
            OwnerId     = visit.Pet.OwnerId,
            PetId       = visit.PetId,
            ScheduledAt = utc,                 // ✅ UTC
            Purpose     = visit.Purpose,
            DoctorId    = visit.DoctorId,
        });

        return;
    }

    foreach (var p in plans)
    {
        if (p == null || p.Date == default)
            continue;

        _db.VisitPlans.Add(new VisitPlan
        {
            VisitId  = visit.Id,
            Date     = p.Date,
            Purpose  = p.Purpose,
            DoctorId = p.DoctorId,
        });

        _db.Reminders.Add(new Reminder
        {
            VisitId = visit.Id,
            DueDate = p.Date.AddDays(-1),
            Status  = ReminderStatus.Pending
        });

        var utc = IstanbulDateOnlyToUtc(p.Date);

        _db.Appointments.Add(new Appointment
        {
            VisitId     = visit.Id,
            OwnerId     = visit.Pet.OwnerId,
            PetId       = visit.PetId,
            ScheduledAt = utc,                
            Purpose     = p.Purpose,
            DoctorId    = p.DoctorId,
        });
    }
}

private async Task SyncDiseaseDiagnosisForVisit(Visit visit, int? diseaseId, string? statusStr)
{
    var existingDiagnosis = await _db.PetDiagnoses.FirstOrDefaultAsync(pd => pd.VisitId == visit.Id);

    if (!diseaseId.HasValue || diseaseId.Value <= 0)
    {
        if (existingDiagnosis != null)
        {
            _db.PetDiagnoses.Remove(existingDiagnosis);
            await _db.SaveChangesAsync();
        }
        return;
    }

    if (!Enum.TryParse<DiagnosisStatus>(statusStr, true, out var status))
        status = DiagnosisStatus.Aktif;

    if (existingDiagnosis == null)
    {
        existingDiagnosis = new PetDiagnosis
        {
            PetId = visit.PetId,
            VisitId = visit.Id,
            DiseaseId = diseaseId.Value,
            Status = status,
            DiagnosedDate = visit.PerformedAt,
            Notes = "Ziyaret sırasında eklendi."
        };
        _db.PetDiagnoses.Add(existingDiagnosis);
    }
    else
    {
        existingDiagnosis.DiseaseId = diseaseId.Value;
        existingDiagnosis.Status = status;
        existingDiagnosis.UpdatedAt = DateTime.UtcNow;
    }
    await _db.SaveChangesAsync();

    if (status == DiagnosisStatus.Kronik)
    {
         var chronicReminder = await _db.Reminders.FirstOrDefaultAsync(r => r.VisitId == visit.Id && r.DueDate > DateOnly.FromDateTime(visit.PerformedAt.AddMonths(1)));
         if (chronicReminder == null)
         {
             _db.Reminders.Add(new Reminder
             {
                 VisitId = visit.Id,
                 DueDate = DateOnly.FromDateTime(visit.PerformedAt.AddMonths(3)),
                 Status = ReminderStatus.Pending
             });
             await _db.SaveChangesAsync();
         }
    }
}

public class UpdateVisitCollectedDto
{
    public decimal? CollectedAmountTl { get; set; }
    public string? Note { get; set; }
}

[HttpPatch("{id:int}/collected")]
public async Task<IActionResult> UpdateVisitCollected([FromRoute] int id, [FromBody] UpdateVisitCollectedDto dto)
{
    if (dto == null) return BadRequest();
    if (dto.CollectedAmountTl is < 0) return BadRequest("CollectedAmountTl cannot be negative.");

    var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == id);
    if (visit is null) return NotFound();

    // 1) Visit'e yaz (UI'nın kalıcı görmesi için şart)
    visit.CollectedAmountTl = dto.CollectedAmountTl;

    // 2) UserId: tahsilatı yapan
    var actorUserId = _currentUser.UserId ?? visit.CreatedByUserId;
    if (!actorUserId.HasValue)
        return Unauthorized("UserId bulunamadı.");

    // 3) Amount
    var amount = dto.CollectedAmountTl ?? 0m;

    // 4) Idempotent: aynı visit + aynı user için tek satır
    var existing = await _db.LedgerEntries.FirstOrDefaultAsync(x =>
        x.VisitId == visit.Id &&
        x.UserId == actorUserId.Value &&
        x.IsIncome == true &&
        x.Category == "VisitCollected"
    );

    if (amount <= 0m)
    {
        if (existing != null) _db.LedgerEntries.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    var note = string.IsNullOrWhiteSpace(dto.Note)
        ? $"Visit collected (VisitId={visit.Id})"
        : dto.Note.Trim();

    if (existing == null)
    {
        _db.LedgerEntries.Add(new LedgerEntry
        {
            UserId = actorUserId.Value,
            VisitId = visit.Id,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = amount,
            IsIncome = true,
            Category = "VisitCollected",
            Note = note,
            CreatedAt = DateTime.UtcNow
        });
    }
    else
    {
        existing.Amount = amount;
        existing.Note = note;
        existing.Date = DateOnly.FromDateTime(DateTime.UtcNow);
        // existing.CreatedAt dokunma
    }

    await _db.SaveChangesAsync();
    
    // 5) Sync Ledger
    await SyncLedgerForVisit(visit);

    return NoContent();
}

[HttpPatch("{id:int}/status")]
public async Task<IActionResult> UpdateVisitStatus(int id, [FromBody] VisitStatusUpdateDto dto)
{
    var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == id);
    if (visit is null) return NotFound();

    if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
        return BadRequest("Status zorunludur.");

    // string -> enum
    Visit.VisitStatus newStatus;
    switch (dto.Status.Trim().ToLowerInvariant())
    {
        case "completed":
        case "yapildi":
            newStatus = Visit.VisitStatus.Completed;
            break;

        case "missed":
        case "yapilmadi":
            newStatus = Visit.VisitStatus.Missed;
            break;

        case "pending":
            newStatus = Visit.VisitStatus.Pending;
            break;

        default:
            return BadRequest("Geçersiz status. (Completed|Missed|Pending)");
    }

    // idempotent
    if (visit.Status == newStatus)
        return NoContent();
    visit.StatusUpdatedAt = DateTime.UtcNow;
    if (newStatus == Visit.VisitStatus.Completed && visit.PerformedAt == default)
        visit.PerformedAt = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    // ✅ Sync Ledger after status change
    await SyncLedgerForVisit(visit);

    return NoContent();
}


[HttpPatch("{id:int}/credit")]
public async Task<IActionResult> UpdateVisitCredit([FromRoute] int id, [FromBody] UpdateVisitCreditDto dto)
{
    if (dto == null) return BadRequest();

    // Negatif olmasın
    if (dto.CreditAmountTl is < 0)
        return BadRequest("CreditAmountTl cannot be negative.");

    var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == id);
    if (visit == null) return NotFound();

    visit.CreditAmountTl = dto.CreditAmountTl;
    await _db.SaveChangesAsync();

    // ✅ Sync Ledger after credit change
    await SyncLedgerForVisit(visit);

    return NoContent();
}

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VisitDto>>> GetVisits(
        [FromQuery] int? ownerId,
        [FromQuery] int? petId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] DateTime? date)
    {
        var query = _db.Visits
            .Include(v => v.Pet)
                .ThenInclude(p => p.Owner)
            .Include(v => v.CreatedByUser)
            .Include(v => v.Doctor)
            .Include(v => v.Images)
            .Include(v => v.Plans)
                .ThenInclude(p => p.Doctor)
            .Include(v => v.Diagnoses)
                .ThenInclude(d => d.Disease)
            .AsQueryable();

        if (ownerId.HasValue)
            query = query.Where(v => v.Pet.OwnerId == ownerId.Value);

        if (petId.HasValue)
            query = query.Where(v => v.PetId == petId.Value);

        if (startDate.HasValue)
            query = query.Where(v => v.PerformedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(v => v.PerformedAt <= endDate.Value);

        if (date.HasValue)
        {
            var dayStart = date.Value.Date;
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);
            query = query.Where(v => v.PerformedAt >= dayStart && v.PerformedAt <= dayEnd);
        }

        var visits = await query
            .OrderByDescending(v => v.PerformedAt)
            .Select(v => new VisitDto
            {
                Id = v.Id,
                PetId = v.PetId,
                PetName = v.Pet.Name,
                OwnerId = v.Pet.OwnerId,
                OwnerName = v.Pet.Owner.FullName,
                OwnerPhone = v.Pet.Owner.PhoneE164,
                PerformedAt = v.PerformedAt,
                Procedures = v.Procedures,
                AmountTl = v.AmountTl,
                Notes = v.Notes,
                ClientNotes = v.ClientNotes,
                NextDate = v.NextDate,
                Purpose = v.Purpose,
                DoctorId = v.DoctorId,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : null,
                CreatedByUserId = v.CreatedByUserId,
                CreditAmountTl = v.CreditAmountTl,
                CollectedAmountTl = v.CollectedAmountTl,
                CreatedByUsername = v.CreatedByUsername,
                CreatedByName = v.CreatedByName,
                ImageUrl = v.ImageUrl,
                MicrochipNumber = v.MicrochipNumber,
                DiseaseId = v.Diagnoses.FirstOrDefault() != null ? (int?)v.Diagnoses.FirstOrDefault()!.DiseaseId : null,
                DiseaseName = v.Diagnoses.FirstOrDefault() != null ? v.Diagnoses.FirstOrDefault()!.Disease!.Name : null,
                DiagnosisStatus = v.Diagnoses.FirstOrDefault() != null ? v.Diagnoses.FirstOrDefault()!.Status.ToString() : null,

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
    var v = await _db.Visits
        .AsNoTracking()
        .Include(x => x.Pet)
            .ThenInclude(p => p.Owner)
        .Include(x => x.CreatedByUser)
        .Include(x => x.Doctor)
        .Include(x => x.Images)
        .Include(x => x.Plans)
            .ThenInclude(p => p.Doctor)
        .Include(x => x.Diagnoses)
            .ThenInclude(d => d.Disease)
        .FirstOrDefaultAsync(x => x.Id == id);

    if (v == null) return NotFound();

    var dto = new VisitDto
    {
        Id = v.Id,
        PetId = v.PetId,
        PetName = v.Pet?.Name ?? "—",
        OwnerId = v.Pet?.OwnerId ?? 0,
        OwnerName = v.Pet?.Owner?.FullName ?? "—",
        OwnerPhone = v.Pet?.Owner?.PhoneE164 ?? "",
        PerformedAt = v.PerformedAt,
        Procedures = v.Procedures,
        AmountTl = v.AmountTl,
        Notes = v.Notes,
        ClientNotes = v.ClientNotes,
        NextDate = v.NextDate,
        Purpose = v.Purpose,
        CreatedByUserId = v.CreatedByUserId,
        CreatedByUsername = v.CreatedByUser?.Username ?? v.CreatedByUsername,
        CreatedByName = v.CreatedByUser?.FullName ?? v.CreatedByName,
        DoctorId = v.DoctorId,
        DoctorName = v.Doctor != null ? v.Doctor.FullName : null,
        ImageUrl = v.ImageUrl,
        MicrochipNumber = v.MicrochipNumber,
        DiseaseId = v.Diagnoses?.FirstOrDefault()?.DiseaseId,
        DiseaseName = v.Diagnoses?.FirstOrDefault()?.Disease?.Name,
        DiagnosisStatus = v.Diagnoses?.FirstOrDefault()?.Status.ToString(),
        CollectedAmountTl = v.CollectedAmountTl,
        CreditAmountTl = v.CreditAmountTl,

        Images = (v.Images ?? new List<VisitImage>())
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new VisitImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                CreatedAt = i.CreatedAt
            })
            .ToList(),

        Plans = (v.Plans ?? new List<VisitPlan>())
            .OrderBy(p => p.Date)
            .Select(p => new VisitPlanDto
            {
                Date = p.Date,
                Purpose = p.Purpose,
                DoctorId = p.DoctorId,
                DoctorName = p.Doctor != null ? p.Doctor.FullName : null
            })
            .ToList()
    };

    return Ok(dto);
}

    [HttpPost]
    public async Task<ActionResult<VisitDto>> CreateVisit([FromBody] VisitCreateDto dto)
    {
        Console.WriteLine("===== CreateVisit CALLED =====");

        try
        {
            Console.WriteLine("DTO: " + JsonSerializer.Serialize(dto));
            Console.WriteLine($"CreateVisit called: Procedures='{dto.Procedures}'");

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
                ClientNotes = dto.ClientNotes,
                NextDate    = primaryNextDate ?? dto.NextDate,
                Purpose     = dto.Purpose,
                CreatedByUserId   = _currentUser.UserId,
                CreatedByUsername = _currentUser.Username,
                CreatedByName     = _currentUser.FullName,
                CreditAmountTl = dto.CreditAmountTl,
                CollectedAmountTl = dto.CollectedAmountTl ?? Math.Max(0m, (dto.AmountTl ?? 0m) - (dto.CreditAmountTl ?? 0m)),
                MicrochipNumber   = dto.MicrochipNumber,
                Status = dto.Status.HasValue ? (Visit.VisitStatus)dto.Status.Value : Visit.VisitStatus.Pending
            };

            var userId = _currentUser.UserId;
            if (userId.HasValue)
                visit.CreatedByUserId = userId.Value;

            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            await _db.Entry(visit).Reference(v => v.Pet).LoadAsync();
            await _db.Entry(visit.Pet).Reference(p => p.Owner).LoadAsync();

            SyncRemindersForVisit(visit, (dto.Plans != null && dto.Plans.Count > 0) ? dto.Plans : null);
            await SyncDiseaseDiagnosisForVisit(visit, dto.DiseaseId, dto.DiagnosisStatus);

            // ✅ Sync Ledger (Ziyaret tamamlandıysa veya tahsilat varsa)
            await SyncLedgerForVisit(visit);

            await _db.SaveChangesAsync();

            var result = new VisitDto
            {
                Id          = visit.Id,
                PetId       = visit.PetId,
                PetName     = pet.Name,
                OwnerId          = pet.OwnerId,
                OwnerName        = pet.Owner.FullName,
                OwnerPhone       = pet.Owner.PhoneE164,
                PerformedAt      = visit.PerformedAt,
                Procedures  = visit.Procedures,
                AmountTl    = visit.AmountTl,
                Notes       = visit.Notes,
                ClientNotes = visit.ClientNotes,
                NextDate    = visit.NextDate,
                Purpose     = visit.Purpose,
                DoctorName  = visit.Doctor != null ? visit.Doctor.FullName : null,
                CreatedByUserId   = visit.CreatedByUserId,
                CreatedByUsername = visit.CreatedByUsername,
                CreatedByName     = visit.CreatedByName,
                ImageUrl          = visit.ImageUrl,
                MicrochipNumber   = visit.MicrochipNumber,
                DiseaseId         = dto.DiseaseId,
                DiagnosisStatus   = dto.DiagnosisStatus,
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
        visit.CreditAmountTl  = dto.CreditAmountTl;
        visit.CollectedAmountTl = dto.CollectedAmountTl ?? Math.Max(0m, (dto.AmountTl ?? 0m) - (dto.CreditAmountTl ?? 0m));
        visit.Notes           = dto.Notes;
        visit.ClientNotes     = dto.ClientNotes;
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


        SyncRemindersForVisit(visit, (dto.Plans != null && dto.Plans.Count > 0) ? dto.Plans : null);
        await SyncDiseaseDiagnosisForVisit(visit, dto.DiseaseId, dto.DiagnosisStatus);
        
        await _db.SaveChangesAsync();

        // ✅ Sync Ledger
        await SyncLedgerForVisit(visit);

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

        var ledgerEntries = _db.LedgerEntries.Where(l => l.VisitId == id);
        _db.LedgerEntries.RemoveRange(ledgerEntries);

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
    try
    {
        var visit = await _db.Visits.FindAsync(id);
        if (visit is null) return NotFound();

        if (files == null || files.Count == 0)
            return BadRequest("Dosya yok.");

        var created = new List<VisitImage>();
        string? lastUrl = null;

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();

            var url = await _storage.UploadVisitImageAsync(
                visitId: visit.Id,
                stream: stream,
                contentType: file.ContentType ?? "application/octet-stream"
            );

            lastUrl = url;

            var image = new VisitImage
            {
                VisitId = visit.Id,
                ImageUrl = url,
                CreatedAt = DateTime.UtcNow,
                // CreatedByUserId varsa burada setleyebilirsin
            };

            created.Add(image);
        }

        // toplu ekle
        _db.VisitImages.AddRange(created);

        // tek kural: her zaman en son yüklenen
        if (!string.IsNullOrWhiteSpace(lastUrl))
            visit.ImageUrl = lastUrl;

        await _db.SaveChangesAsync();

        var results = created
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new VisitImageDto
            {
                Id = x.Id,
                ImageUrl = x.ImageUrl,
                CreatedAt = x.CreatedAt
            })
            .ToList();

        return Ok(results);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[UploadImages Error]: {ex.ToString()}");
        return StatusCode(500, new { message = "Görsel yüklenirken sunucu hatası oluştu.", error = ex.ToString() });
    }
}

private async Task SyncLedgerForVisit(Visit visit)
{
    Console.WriteLine($"[SyncLedger] VisitId={visit.Id}, Status={visit.Status}, Amount={visit.AmountTl}, Credit={visit.CreditAmountTl}, Collected={visit.CollectedAmountTl}");

    var userId = _currentUser.UserId ?? visit.CreatedByUserId;
    if (!userId.HasValue) 
    {
        Console.WriteLine("[SyncLedger] SKIPPED: No UserId found.");
        return;
    }

    // 1) Handle VisitIncome (Revenue/Ciro)
    // Rule: If AmountTl > 0, we have revenue. (Doesn't necessarily need to be Completed, but usually it is)
    var billableAmount = visit.AmountTl ?? 0m;
    var existingIncome = await _db.LedgerEntries
        .FirstOrDefaultAsync(x => x.VisitId == visit.Id && x.Category == "VisitIncome");

    if (billableAmount <= 0m)
    {
        if (existingIncome != null) _db.LedgerEntries.Remove(existingIncome);
    }
    else
    {
        if (existingIncome == null)
        {
            _db.LedgerEntries.Add(new LedgerEntry
            {
                UserId = userId.Value,
                VisitId = visit.Id,
                Date = DateOnly.FromDateTime(visit.PerformedAt.Date),
                Amount = billableAmount,
                IsIncome = true,
                Category = "VisitIncome",
                Note = $"Ziyaret Tahakkuk (VisitId={visit.Id})",
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingIncome.Amount = billableAmount;
            existingIncome.Date = DateOnly.FromDateTime(visit.PerformedAt.Date);
        }
    }

    // 2) Handle VisitCollected (Actual Cash Flow)
    // Rule: If CollectedAmountTl > 0, we have cash flow.
    var collectedAmount = visit.CollectedAmountTl ?? 0m;
    var existingCollected = await _db.LedgerEntries
        .FirstOrDefaultAsync(x => x.VisitId == visit.Id && x.Category == "VisitCollected");

    if (collectedAmount <= 0m)
    {
        if (existingCollected != null) _db.LedgerEntries.Remove(existingCollected);
    }
    else
    {
        if (existingCollected == null)
        {
            _db.LedgerEntries.Add(new LedgerEntry
            {
                UserId = userId.Value,
                VisitId = visit.Id,
                Date = DateOnly.FromDateTime(visit.PerformedAt.Date),
                Amount = collectedAmount,
                IsIncome = true,
                Category = "VisitCollected",
                Note = $"Ziyaret Tahsilat (VisitId={visit.Id})",
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingCollected.Amount = collectedAmount;
            existingCollected.Date = DateOnly.FromDateTime(visit.PerformedAt.Date);
        }
    }

    // 3) Cleanup old "Visit" category (Migration)
    var legacy = await _db.LedgerEntries
        .Where(x => x.VisitId == visit.Id && x.Category == "Visit")
        .ToListAsync();
    if (legacy.Any()) _db.LedgerEntries.RemoveRange(legacy);

    await _db.SaveChangesAsync();
}

private static TimeZoneInfo GetIstanbulTimeZone()
{
    try
    {
        // Linux
        return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
    }
    catch
    {
        // Windows
        return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
    }
}

private static DateTime IstanbulDateOnlyToUtc(DateOnly date, int hour = 10, int minute = 30)
{
    var tz = GetIstanbulTimeZone();

    var local = new DateTime(
        date.Year,
        date.Month,
        date.Day,
        hour,
        minute,
        0,
        DateTimeKind.Unspecified
    );

    return TimeZoneInfo.ConvertTimeToUtc(local, tz);
}

}
