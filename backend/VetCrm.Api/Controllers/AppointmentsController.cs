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
            if (request.OwnerId <= 0)
                return BadRequest("Hasta sahibi (OwnerId) zorunludur.");

            if (request.PetIds == null || request.PetIds.Count == 0)
                return BadRequest("En az bir hayvan seçilmelidir.");

            if (request.VisitId == null || request.VisitId <= 0)
                return BadRequest("VisitId zorunludur (randevu, bir ziyaret kaydına bağlı olmalıdır).");

            var currentUserId = _currentUser.UserId;
            if (currentUserId == null)
                return Unauthorized("Oturum geçersiz. Lütfen tekrar giriş yapın.");

            // 2) Saat aralığı kontrolü (10:30 - 19:30)
            var startTime = new TimeSpan(10, 30, 0);
            var endTime   = new TimeSpan(19, 30, 0);
            var timeOfDay = request.ScheduledAt.TimeOfDay;

            if (timeOfDay < startTime || timeOfDay > endTime)
            {
                return BadRequest("Randevu saati 10:30 - 19:30 arasında olmalıdır.");
            }

            var scheduledDateOnly = DateOnly.FromDateTime(request.ScheduledAt);

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
            {
                return BadRequest("Seçilen hayvanlardan en az biri bu hasta sahibine ait değil.");
            }

            // 4) Visit üzerinde mikroçip güncellemesi (geldiyse)
            if (!string.IsNullOrWhiteSpace(request.MicrochipNumber))
            {
                visit.MicrochipNumber = request.MicrochipNumber;
            }

            // İstersen, en yakın gelecek randevuyu Visit.NextDate olarak tutabilirsin
            // (Şu an hepsi aynı tarihte olduğu için direkt set ediyoruz)
            visit.NextDate = scheduledDateOnly;

            var now = DateTime.UtcNow;

            // 5) Appointment + Reminder kayıtları
            var createdAppointments = new List<Appointment>();

            foreach (var petId in validPetIds)
            {
                var appointment = new Appointment
                {
                    OwnerId = request.OwnerId,
                    PetId = petId,
                    ScheduledAt = request.ScheduledAt,
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
                    Status = 0,              // Pending
                    IsCompleted = false,
                    SentAt = null,
                    CompletedAt = null
                };

                _db.Reminders.Add(reminder);
            }

            // 6) Bildirim metni
            var petNames = await _db.Pets
                .Where(p => validPetIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToListAsync();

            var petsText = petNames.Count > 0
                ? string.Join(", ", petNames)
                : "Hasta";

            var ownerName = owner.FullName ?? "Hasta Sahibi";

            var message =
                $"{ownerName} - {petsText} için " +
                $"{request.ScheduledAt:dd.MM.yyyy HH:mm} tarihine randevu oluşturuldu. " +
                $"İşlem: {request.Purpose ?? "Belirtilmedi"}";

            // 7) Tüm kullanıcılara Notification ekle
            var allUsers = await _db.Users.ToListAsync();

            foreach (var user in allUsers)
            {
                var notification = new Notification
                {
                    UserId = user.Id,
                    Type = "AppointmentCreated",
                    Message = message,
                    VisitId = visit.Id,
                    CreatedAt = now,
                    IsRead = false
                };

                _db.Notifications.Add(notification);
            }

            // 8) Tüm değişiklikleri kaydet
            await _db.SaveChangesAsync();

            return Ok(new
            {
                appointmentIds = createdAppointments.Select(a => a.Id).ToList(),
                visitId = visit.Id
            });
        }
    }
}
