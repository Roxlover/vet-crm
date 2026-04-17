using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
namespace VetCrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{   


   public class PetProfileDto
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
    public string? OwnerPhoneE164 { get; set; }

    public string Name { get; set; } = "";
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int? AgeYears { get; set; }
    public int? AgeMonths { get; set; }
    public string? Notes { get; set; }

    public List<PetVisitDto> Visits { get; set; } = new();
}

public class PetVisitDto
{
    public int VisitId { get; set; }
    public DateTime PerformedAt { get; set; }

    public string? Procedures { get; set; }
    public string? Notes { get; set; }

    public decimal? AmountTl { get; set; }
    public decimal? CreditAmountTl { get; set; }

    public string? CreatedByUsername { get; set; }
    public string? CreatedByName { get; set; }

    public List<PetVisitImageDto> Images { get; set; } = new();
}

public class PetVisitImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = "";
}

    private static (int years, int months)? CalcAge(DateOnly? birthDate)
{
    if (birthDate is null) return null;

    var today = DateOnly.FromDateTime(DateTime.Today);

    var years = today.Year - birthDate.Value.Year;
    var months = today.Month - birthDate.Value.Month;

    if (today.Day < birthDate.Value.Day)
        months--;

    if (months < 0)
    {
        years--;
        months += 12;
    }

    if (years < 0) years = 0;
    if (months < 0) months = 0;

    return (years, months);
}

    
    private readonly VetCrmDbContext _db;

    public PetsController(VetCrmDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PetDto>>> GetPets([FromQuery] int? ownerId)
    {
        var query = _db.Pets
            .Include(p => p.Owner)
            .AsQueryable();

        if (ownerId.HasValue)
        {
            query = query.Where(p => p.OwnerId == ownerId.Value);
        }

        var pets = await query
            .Select(p => new PetDto
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner.FullName,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                BirthDate = p.BirthDate,
                Notes = p.Notes
            })
            .ToListAsync();

        return Ok(pets);
    }

    [HttpGet("{id:int}/profile")]
public async Task<ActionResult<PetProfileDto>> GetPetProfile(int id)
{
    var petBase = await _db.Pets
        .Include(p => p.Owner)
        .Where(p => p.Id == id)
        .Select(p => new
        {
            p.Id,
            p.OwnerId,
            OwnerName = p.Owner.FullName,
            OwnerPhone = p.Owner.PhoneE164,
            p.Name,
            p.Species,
            p.Breed,
            p.BirthDate,
            p.Notes
        })
        .FirstOrDefaultAsync();

    if (petBase is null)
        return NotFound();

    var visits = await _db.Visits
        .Where(v => v.PetId == id)
        .OrderByDescending(v => v.PerformedAt)
        .Select(v => new PetVisitDto
        {
            VisitId = v.Id,
            PerformedAt = v.PerformedAt,
            Procedures = v.Procedures,
            Notes = v.Notes,
            AmountTl = v.AmountTl,
            CreditAmountTl = v.CreditAmountTl,

            // Eğer compile hatası olursa buradaki navigation adını düzeltiriz
            CreatedByUsername = v.CreatedByUser != null ? v.CreatedByUser.Username : null,
            CreatedByName = v.CreatedByUser != null ? v.CreatedByUser.FullName : null,

            Images = _db.VisitImages
                .Where(img => img.VisitId == v.Id)
                .OrderBy(img => img.Id)
                .Select(img => new PetVisitImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl
                })
                .ToList()
        })
        .ToListAsync();

    var age = CalcAge(petBase.BirthDate);
    var dto = new PetProfileDto
    {
        Id = petBase.Id,
        OwnerId = petBase.OwnerId,
        OwnerName = petBase.OwnerName,
        OwnerPhoneE164 = petBase.OwnerPhone,

        Name = petBase.Name,
        Species = petBase.Species,
        Breed = petBase.Breed,
        BirthDate = petBase.BirthDate,
        AgeYears = age?.years,
        AgeMonths = age?.months,
        Notes = petBase.Notes,
        Visits = visits
    };

    return Ok(dto);
}


    [HttpGet("{id:int}")]
    public async Task<ActionResult<PetDto>> GetPet(int id)
    {
        var pet = await _db.Pets
            .Include(p => p.Owner)
            .Where(p => p.Id == id)
            .Select(p => new PetDto
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner.FullName,
                Name = p.Name,
                Species = p.Species,
                Breed = p.Breed,
                BirthDate = p.BirthDate,
                Notes = p.Notes
            })
            .FirstOrDefaultAsync();

        if (pet is null)
            return NotFound();

        return Ok(pet);
    }
 
    [HttpPost]
    public async Task<ActionResult<PetDto>> CreatePet([FromBody] PetCreateDto dto)
    {
        var ownerExists = await _db.Owners.AnyAsync(o => o.Id == dto.OwnerId);
        if (!ownerExists)
        {
            return BadRequest($"Owner with id {dto.OwnerId} not found.");
        }

        var pet = new Pet
        {
            OwnerId = dto.OwnerId,
            Name = dto.Name,
            Species = dto.Species,
            Breed = dto.Breed,
            BirthDate = dto.BirthDate,
            Notes = dto.Notes
        };

        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();

        var owner = await _db.Owners.FindAsync(pet.OwnerId);

        var result = new PetDto
        {
            Id = pet.Id,
            OwnerId = pet.OwnerId,
            OwnerName = owner!.FullName,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            BirthDate = pet.BirthDate,
            Notes = pet.Notes
        };

        return CreatedAtAction(nameof(GetPet), new { id = pet.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePet(int id, [FromBody] PetUpdateDto dto)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet is null)
            return NotFound();

        pet.Name = dto.Name;
        pet.Species = dto.Species;
        pet.Breed = dto.Breed;
        pet.BirthDate = dto.BirthDate;
        pet.Notes = dto.Notes;

        await _db.SaveChangesAsync();

        return NoContent();
    }
    [HttpDelete("{id:int}")]
public async Task<IActionResult> DeletePet(int id)
{
    var pet = await _db.Pets.FindAsync(id);
    if (pet is null)
        return NotFound();

    var hasVisits = await _db.Visits.AnyAsync(v => v.PetId == id);
    if (hasVisits)
        return BadRequest("Bu hayvana ait ziyaret kayıtları var. Silinemez. (Gerekirse pasife alma ekleyelim.)");

    // Eğer appointment tablosunda PetId varsa onu da kontrol et:
    var hasAppointments = await _db.Appointments.AnyAsync(a => a.PetId == id);
    if (hasAppointments)
        return BadRequest("Bu hayvana ait randevu kayıtları var. Silinemez. (Gerekirse pasife alma ekleyelim.)");

    _db.Pets.Remove(pet);
    await _db.SaveChangesAsync();

    return NoContent();
}


}
