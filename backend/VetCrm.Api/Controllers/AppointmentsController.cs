using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Api.Services;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly VetCrmDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public AppointmentsController(VetCrmDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            Console.WriteLine("===== AppointmentsController.Create CALLED =====");
            try
            {
                if (request == null) return BadRequest("Request body is null.");
                Console.WriteLine($"Request: OwnerId={request.OwnerId}, PetCount={request.PetIds?.Count}, ScheduledAt={request.ScheduledAt}");

                if (request.OwnerId <= 0)
                    return BadRequest("Hasta sahibi (OwnerId) zorunludur.");

                if (request.PetIds == null || request.PetIds.Count == 0)
                    return BadRequest("En az bir hayvan seçilmelidir.");

                var currentUserId = _currentUser.UserId;
                if (currentUserId == null)
                    return Unauthorized("Oturum geçersiz. Lütfen tekrar giriş yapın.");

                var now = DateTime.UtcNow;
                TimeZoneInfo tz;
                try { tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
                catch { tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }

                var localIstanbul = TimeZoneInfo.ConvertTimeFromUtc(request.ScheduledAt.ToUniversalTime(), tz);
                var utc = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(localIstanbul, tz), DateTimeKind.Utc);
                var scheduledDateOnly = DateOnly.FromDateTime(localIstanbul);

                Console.WriteLine($"Time conversion: Local={localIstanbul}, UTC={utc}");

                Visit? visit = null;
                if (request.VisitId.HasValue && request.VisitId.Value > 0)
                {
                    visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == request.VisitId.Value);
                    if (visit == null)
                        return BadRequest("Geçersiz visit.");
                    Console.WriteLine($"Linked to existing VisitId={visit.Id}");
                }

                var owner = await _db.Owners.FirstOrDefaultAsync(o => o.Id == request.OwnerId);
                if (owner == null)
                    return BadRequest("Geçersiz hasta sahibi (owner).");

                var distinctPetIds = request.PetIds.Distinct().ToList();
                var validPetIds = await _db.Pets
                    .Where(p => p.OwnerId == request.OwnerId && distinctPetIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                Console.WriteLine($"Valid Pets Count: {validPetIds.Count}");

                var createdAppointments = new List<Appointment>();

                foreach (var petId in validPetIds)
                {
                    var targetVisit = visit;
                    if (targetVisit == null)
                    {
                        Console.WriteLine($"Creating auto-visit for PetId={petId}");
                        targetVisit = new Visit
                        {
                            PetId = petId,
                            PerformedAt = utc,
                            Purpose = request.Purpose,
                            Procedures = request.Procedures,
                            AmountTl = request.AmountTl,
                            CreditAmountTl = request.CreditAmountTl,
                            Notes = request.Notes,
                            Status = (request.AmountTl > 0) ? Visit.VisitStatus.Completed : Visit.VisitStatus.Pending,
                            CreatedByUserId = currentUserId,
                            CreatedByUsername = _currentUser.Username,
                            CreatedByName = _currentUser.FullName,
                            NextDate = scheduledDateOnly
                        };
                        _db.Visits.Add(targetVisit);
                    }
                    else
                    {
                        // Mevcut visit varsa bilgileri güncelle (opsiyonel ama mantıklı)
                        if (request.AmountTl.HasValue) targetVisit.AmountTl = request.AmountTl;
                        if (request.CreditAmountTl.HasValue) targetVisit.CreditAmountTl = request.CreditAmountTl;
                        if (!string.IsNullOrWhiteSpace(request.Procedures)) targetVisit.Procedures = request.Procedures;
                        if (!string.IsNullOrWhiteSpace(request.Notes)) targetVisit.Notes = request.Notes;
                    }

                    var appointment = new Appointment
                    {
                        OwnerId = request.OwnerId,
                        PetId = petId,
                        ScheduledAt = utc,
                        Purpose = request.Purpose,
                        DoctorId = request.DoctorId,
                        Visit = targetVisit
                    };

                    createdAppointments.Add(appointment);
                    _db.Appointments.Add(appointment);

                    var reminder = new Reminder
                    {
                        Visit = targetVisit,
                        DueDate = scheduledDateOnly,
                        CreatedAt = now,
                        Status = 0,
                        IsCompleted = false
                    };
                    _db.Reminders.Add(reminder);
                }

                Console.WriteLine("Saving context...");
                await _db.SaveChangesAsync();
                Console.WriteLine("Save successful.");

                var petNames = await _db.Pets
                    .Where(p => validPetIds.Contains(p.Id))
                    .Select(p => p.Name)
                    .ToListAsync();

                var petsText = petNames.Count > 0 ? string.Join(", ", petNames) : "Hasta";
                var ownerName = owner.FullName ?? "Hasta Sahibi";
                var message = $"{ownerName} - {petsText} için {localIstanbul:dd.MM.yyyy HH:mm} tarihine randevu oluşturuldu. İşlem: {request.Purpose ?? "Belirtilmedi"}";

                var allUsers = await _db.Users.ToListAsync();
                foreach (var user in allUsers)
                {
                    _db.Notifications.Add(new Notification
                    {
                        UserId = user.Id,
                        Type = "AppointmentCreated",
                        Message = message,
                        VisitId = createdAppointments.FirstOrDefault()?.VisitId,
                        CreatedAt = now,
                        IsRead = false
                    });
                }
                
                // ✅ Sync Ledger for each visit
                foreach (var v in createdAppointments.Select(a => a.Visit).Distinct())
                {
                    if (v != null) await SyncLedgerForVisit(v);
                }

                await _db.SaveChangesAsync();
                Console.WriteLine("Final save successful. Returning OK.");

                return Ok(new
                {
                    appointmentIds = createdAppointments.Select(a => a.Id).ToList(),
                    visitId = createdAppointments.FirstOrDefault()?.VisitId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== AppointmentsController.Create ERROR =====");
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentRequest request)
        {
            if (request == null) return BadRequest();

            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var utc = EnsureUtc(request.ScheduledAt);
            appointment.ScheduledAt = utc;
            appointment.Purpose = request.Purpose;
            appointment.DoctorId = request.DoctorId;

            // Eğer bir Visit'e bağlıysa, Visit'in NextDate bilgisini de güncellemeliyiz
            if (appointment.VisitId.HasValue)
            {
                var visit = await _db.Visits.FindAsync(appointment.VisitId.Value);
                if (visit != null)
                {
                    var localIstanbul = UtcToIstanbul(utc);
                    visit.NextDate = DateOnly.FromDateTime(localIstanbul);
                }
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static DateTime EnsureUtc(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc) return dt;
            if (dt.Kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return dt.ToUniversalTime();
        }

        private static TimeZoneInfo GetIstanbulTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
        }

        private static DateTime UtcToIstanbul(DateTime utc)
        {
            var tz = GetIstanbulTimeZone();
            return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        }

        private async Task SyncLedgerForVisit(Visit visit)
        {
            var userId = _currentUser.UserId ?? visit.CreatedByUserId;
            if (!userId.HasValue) return;

            var existing = await _db.LedgerEntries
                .Where(x => x.VisitId == visit.Id && x.Category == "Visit")
                .ToListAsync();

            var total = visit.AmountTl ?? 0m;
            var credit = visit.CreditAmountTl ?? 0m;
            var income = Math.Max(0m, total - credit);

            if (visit.Status != Visit.VisitStatus.Completed || income <= 0m)
            {
                if (existing.Any())
                {
                    _db.LedgerEntries.RemoveRange(existing);
                }
                return;
            }

            if (!existing.Any())
            {
                _db.LedgerEntries.Add(new LedgerEntry
                {
                    UserId = userId.Value,
                    VisitId = visit.Id,
                    Date = DateOnly.FromDateTime(visit.PerformedAt.Date),
                    Amount = income,
                    IsIncome = true,
                    Category = "Visit",
                    Note = $"Ziyaret Geliri (VisitId={visit.Id})",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                var first = existing.First();
                first.Amount = income;
                first.Date = DateOnly.FromDateTime(visit.PerformedAt.Date);
                if (existing.Count > 1)
                {
                    _db.LedgerEntries.RemoveRange(existing.Skip(1));
                }
            }
        }
    }

    public class UpdateAppointmentRequest
    {
        public DateTime ScheduledAt { get; set; }
        public string? Purpose { get; set; }
        public int? DoctorId { get; set; }
    }
}
