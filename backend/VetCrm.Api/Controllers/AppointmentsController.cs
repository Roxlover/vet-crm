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
            // 1) Giriş kontrolleri
            if (request == null)
                return BadRequest("İstek gövdesi (request) zorunludur.");

            if (request.OwnerId <= 0)
                return BadRequest("Hasta sahibi (OwnerId) zorunludur.");

            if (request.PetIds == null || request.PetIds.Count == 0)
                return BadRequest("En az bir hayvan seçilmelidir.");

            if (request.VisitId == null || request.VisitId <= 0)
                return BadRequest("VisitId zorunludur (randevu, bir ziyaret kaydına bağlı olmalıdır).");

            var currentUserId = _currentUser.UserId;
            if (currentUserId == null)
                return Unauthorized("Oturum geçersiz. Lütfen tekrar giriş yapın.");

            // 2) Saat aralığı kontrolü (İstanbul saatine göre 10:30 - 19:30)
            var utc = EnsureUtc(request.ScheduledAt);
            var localIstanbul = UtcToIstanbul(utc);

            if (!IsWithinWorkingHours(localIstanbul))
                return BadRequest("Randevu saati 10:30 - 19:30 arasında olmalıdır.");

            var scheduledDateOnly = DateOnly.FromDateTime(localIstanbul);

            // 3) İlgili kayıtlar gerçekten var mı?
            var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == request.VisitId.Value);
            if (visit == null)
                return BadRequest("Geçersiz visit.");

            var owner = await _db.Owners.FirstOrDefaultAsync(o => o.Id == request.OwnerId);
            if (owner == null)
                return BadRequest("Geçersiz hasta sahibi (owner).");

            // Pet’ler gerçekten bu owner’a mı ait?
            var distinctPetIds = request.PetIds.Distinct().ToList();

            var validPetIds = await _db.Pets
                .Where(p => p.OwnerId == request.OwnerId && distinctPetIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            if (validPetIds.Count != distinctPetIds.Count)
                return BadRequest("Seçilen hayvanlardan en az biri bu hasta sahibine ait değil.");

            // Mikroçip (opsiyonel)
            if (!string.IsNullOrWhiteSpace(request.MicrochipNumber))
                visit.MicrochipNumber = request.MicrochipNumber;

            // Visit.NextDate: local gün
            visit.NextDate = scheduledDateOnly;

            var now = DateTime.UtcNow;
            var createdAppointments = new List<Appointment>();

            foreach (var petId in validPetIds)
            {
                // 4) Duplicate engeli: aynı Visit + aynı Pet + aynı ScheduledAt (UTC)
                var exists = await _db.Appointments.AnyAsync(a =>
                    a.VisitId == request.VisitId.Value &&
                    a.PetId == petId &&
                    a.ScheduledAt == utc
                );

                if (exists)
                    return BadRequest("Aynı randevu zaten mevcut (aynı ziyaret + aynı hayvan + aynı saat).");

                var appointment = new Appointment
                {
                    OwnerId = request.OwnerId,
                    PetId = petId,
                    ScheduledAt = utc,               // DB’de UTC sakla
                    Purpose = request.Purpose,
                    DoctorId = request.DoctorId,
                    VisitId = request.VisitId
                };

                createdAppointments.Add(appointment);
                _db.Appointments.Add(appointment);

                var reminder = new Reminder
                {
                    VisitId = visit.Id,
                    DueDate = scheduledDateOnly,
                    CreatedAt = now,
                    Status = 0,
                    IsCompleted = false,
                    SentAt = null,
                    CompletedAt = null
                };
                _db.Reminders.Add(reminder);
            }

            // 5) Bildirim metni (İstanbul saatine göre)
            var petNames = await _db.Pets
                .Where(p => validPetIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToListAsync();

            var petsText = petNames.Count > 0 ? string.Join(", ", petNames) : "Hasta";
            var ownerName = owner.FullName ?? "Hasta Sahibi";

            var message =
                $"{ownerName} - {petsText} için " +
                $"{localIstanbul:dd.MM.yyyy HH:mm} tarihine randevu oluşturuldu. " +
                $"İşlem: {request.Purpose ?? "Belirtilmedi"}";

            var allUsers = await _db.Users.ToListAsync();
            foreach (var user in allUsers)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Type = "AppointmentCreated",
                    Message = message,
                    VisitId = visit.Id,
                    CreatedAt = now,
                    IsRead = false
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                appointmentIds = createdAppointments.Select(a => a.Id).ToList(),
                visitId = visit.Id
            });
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

        private static bool IsWithinWorkingHours(DateTime localIstanbul)
        {
            var minutes = localIstanbul.Hour * 60 + localIstanbul.Minute;
            var start = 10 * 60 + 30;
            var end = 19 * 60 + 30;
            return minutes >= start && minutes <= end;
        }
    }
}
