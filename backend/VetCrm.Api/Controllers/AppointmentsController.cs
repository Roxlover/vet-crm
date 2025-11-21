using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Infrastructure.Data;
using VetCrm.Domain.Entities;

namespace VetCrm.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly VetCrmDbContext _db;

        public AppointmentsController(VetCrmDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            if (request.PetIds == null || request.PetIds.Count == 0)
            {
                return BadRequest("En az bir hayvan seçilmelidir.");
            }

            // Tarih kısmını DateOnly'e çeviriyoruz (NextDate için)
            var dateOnly = DateOnly.FromDateTime(request.ScheduledAt.Date);

            var visits = new List<Visit>();

            foreach (var petId in request.PetIds.Distinct())
            {
                var visit = new Visit
                {
                    PetId = petId,
                    PerformedAt = request.ScheduledAt,      // şimdilik randevu zamanı
                    NextDate = dateOnly,                    // takvim sekmesi için
                    Purpose = request.Purpose,
                    Procedures = string.Empty,
                    AmountTl = null,
                    Notes = string.Empty,
                    DoctorId = request.DoctorId,
                    CreatedByUserId = request.CreatedByUserId
                };

                _db.Visits.Add(visit);
                visits.Add(visit);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                visitIds = visits.Select(v => v.Id).ToList()
            });
        }
    }
}
