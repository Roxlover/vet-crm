using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using VetCrm.Api.Services;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

[Authorize(Roles = "Client")]
[ApiController]
[Route("api/[controller]")]
public class ClientPortalController : ControllerBase
{
    private readonly VetCrmDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public class ClientProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneE164 { get; set; } = null!;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int PetCount { get; set; }
    }

    public class ClientPetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public DateOnly? BirthDate { get; set; }
        public int? AgeYears { get; set; }
        public int? AgeMonths { get; set; }
        public string? ClientNotes { get; set; }
    }

    public class ClientVisitDto
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = null!;
        public DateTime PerformedAt { get; set; }
        public string? Purpose { get; set; }
        public string? Procedures { get; set; }
        public string? ClientNotes { get; set; }
        public decimal? CollectedAmountTl { get; set; }
        public string? DoctorName { get; set; }
        public List<string> Images { get; set; } = new();
    }

    public class ClientReminderDto
    {
        public int Id { get; set; }
        public string PetName { get; set; } = null!;
        public DateOnly DueDate { get; set; }
        public string? Purpose { get; set; }
        public bool IsCompleted { get; set; }
    }

    public ClientPortalController(VetCrmDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private int GetOwnerId()
    {
        return _currentUser.UserId ?? throw new UnauthorizedAccessException("Müşteri kimliği bulunamadı.");
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ClientProfileDto>> GetProfile()
    {
        var ownerId = GetOwnerId();
        var owner = await _db.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == ownerId);

        if (owner == null)
            return NotFound("Müşteri kaydı bulunamadı.");

        // Sahibin aktif petlerinin toplam borcu (kredi miktarı)
        var petIds = owner.Pets.Where(p => p.IsActive).Select(p => p.Id).ToList();
        var totalCredit = await _db.Visits
            .Where(v => petIds.Contains(v.PetId))
            .SumAsync(v => v.CreditAmountTl) ?? 0;

        return Ok(new ClientProfileDto
        {
            Id = owner.Id,
            FullName = owner.FullName,
            PhoneE164 = owner.PhoneE164,
            Email = owner.Email,
            Address = owner.Address,
            OutstandingBalance = totalCredit,
            PetCount = petIds.Count
        });
    }

    [HttpGet("pets")]
    public async Task<ActionResult<IEnumerable<ClientPetDto>>> GetPets()
    {
        var ownerId = GetOwnerId();
        var pets = await _db.Pets
            .Where(p => p.OwnerId == ownerId && p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new ClientPetDto
            {
                Id = p.Id,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                BirthDate = p.BirthDate,
                AgeYears = p.AgeYears,
                AgeMonths = p.AgeMonths,
                ClientNotes = p.ClientNotes
            })
            .ToListAsync();

        return Ok(pets);
    }

    [HttpGet("visits")]
    public async Task<ActionResult<IEnumerable<ClientVisitDto>>> GetVisits()
    {
        var ownerId = GetOwnerId();
        var petIds = await _db.Pets
            .Where(p => p.OwnerId == ownerId && p.IsActive)
            .Select(p => p.Id)
            .ToListAsync();

        var visits = await _db.Visits
            .Include(v => v.Pet)
            .Include(v => v.Doctor)
            .Include(v => v.Images)
            .Where(v => petIds.Contains(v.PetId))
            .OrderByDescending(v => v.PerformedAt)
            .Select(v => new ClientVisitDto
            {
                Id = v.Id,
                PetId = v.PetId,
                PetName = v.Pet.Name,
                PerformedAt = v.PerformedAt,
                Purpose = v.Purpose,
                Procedures = v.Procedures,
                ClientNotes = v.ClientNotes,
                CollectedAmountTl = v.CollectedAmountTl,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : v.CreatedByName,
                Images = v.Images.Select(img => img.ImageUrl).ToList()
            })
            .ToListAsync();

        return Ok(visits);
    }

    [HttpGet("reminders")]
    public async Task<ActionResult<IEnumerable<ClientReminderDto>>> GetReminders()
    {
        var ownerId = GetOwnerId();
        var petIds = await _db.Pets
            .Where(p => p.OwnerId == ownerId && p.IsActive)
            .Select(p => p.Id)
            .ToListAsync();

        var reminders = await _db.Reminders
            .Include(r => r.Visit)
            .ThenInclude(v => v.Pet)
            .Where(r => petIds.Contains(r.Visit.PetId))
            .OrderBy(r => r.DueDate)
            .Select(r => new ClientReminderDto
            {
                Id = r.Id,
                PetName = r.Visit.Pet.Name,
                DueDate = r.DueDate,
                Purpose = r.Visit.Purpose,
                IsCompleted = r.IsCompleted
            })
            .ToListAsync();

        return Ok(reminders);
    }
}
