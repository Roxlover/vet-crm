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
            if (request.PetIds == null || request.PetIds.Count == 0)
                return BadRequest("En az bir hayvan seçilmelidir.");

            var currentUserId = _currentUser.UserId;
            if (currentUserId == null)
                return Unauthorized("Oturum geçersiz. Lütfen tekrar giriş yapın.");

            // 🔹 Çalışma saatleri kontrolü
            var startTime = new TimeSpan(10, 30, 0);
            var endTime   = new TimeSpan(19, 30, 0);
            var timeOfDay = request.ScheduledAt.TimeOfDay;

            if (timeOfDay < startTime || timeOfDay > endTime)
            {
                return BadRequest("Randevu saati 10:30 - 19:30 arasında olmalıdır.");
            }

            var dateOnly = DateOnly.FromDateTime(request.ScheduledAt.Date);
            var visits = new List<Visit>();

            // 🔹 Ziyaret kayıtlarını oluştur
            foreach (var petId in request.PetIds.Distinct())
            {
                var visit = new Visit
                {
                    PetId = petId,
                    PerformedAt = request.ScheduledAt,
                    NextDate = dateOnly,
                    Purpose = request.Purpose,
                    Procedures = string.Empty,
                    AmountTl = null,
                    Notes = string.Empty,
                    DoctorId = request.DoctorId,
                    CreatedByUserId = currentUserId.Value
                };

                var userId = _currentUser.UserId;
                if (userId.HasValue)
                {
                    visit.CreatedByUserId = userId.Value;
                }
                _db.Visits.Add(visit);
                visits.Add(visit);
            }

            // Önce visit'leri kaydedelim ki Id'leri oluşsun
            await _db.SaveChangesAsync();

            // ─────────────────────────────────────────────
            //  RANDEVU BİLDİRİMİ (TÜM KULLANICILARA)
            // ─────────────────────────────────────────────

            // Sahip bilgisi
            Owner? owner = null;
            if (request.OwnerId > 0)
            {
                owner = await _db.Owners.FindAsync(request.OwnerId);
            }


            // Seçilen pet isimleri
            var petNames = await _db.Pets
                .Where(p => request.PetIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToListAsync();

            var petsText = petNames.Count > 0
                ? string.Join(", ", petNames)
                : "Hasta";

            var ownerName = owner?.FullName ?? "Hasta Sahibi";

            // Mesaj metni
            var message =
                $"{ownerName} - {petsText} için " +
                $"{request.ScheduledAt:dd.MM.yyyy HH:mm} tarihine randevu oluşturuldu. " +
                $"İşlem: {request.Purpose ?? "Belirtilmedi"}";

            var firstVisitId = visits.FirstOrDefault()?.Id;

            // Tüm kullanıcılara notification oluştur
            var allUsers = await _db.Users.ToListAsync();

            foreach (var user in allUsers)
            {
                var notification = new Notification
                {
                    UserId = user.Id,
                    Type = "AppointmentCreated",
                    Message = message,
                    VisitId = firstVisitId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _db.Notifications.Add(notification);
            }

            await _db.SaveChangesAsync();

            // ─────────────────────────────────────────────

            return Ok(new
            {
                visitIds = visits.Select(v => v.Id).ToList()
            });
        }
    }
}
