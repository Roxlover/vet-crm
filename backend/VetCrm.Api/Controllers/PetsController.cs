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
                MicrochipNumber = p.MicrochipNumber,
                Notes = p.Notes,
                ClientNotes = p.ClientNotes
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
                p.MicrochipNumber,
                p.Notes,
                p.ClientNotes
            })
            .FirstOrDefaultAsync();

        if (petBase == null)
            return NotFound();

        var visits = await _db.Visits
            .Where(v => v.PetId == id)
            .OrderByDescending(v => v.PerformedAt)
            .Select(v => new PetVisitDto
            {
                VisitId = v.Id,
                PerformedAt = v.PerformedAt,
                Purpose = v.Purpose,
                Procedures = v.Procedures,
                Notes = v.Notes,
                ClientNotes = v.ClientNotes,
                AmountTl = v.AmountTl,
                CreditAmountTl = v.CreditAmountTl,
                CreatedByUsername = v.CreatedByUsername,
                CreatedByName = v.CreatedByName,
                Images = _db.VisitImages
                    .Where(img => img.VisitId == v.Id)
                    .OrderBy(img => img.Id)
                    .Select(img => new PetVisitImageDto
                    {
                        Id = img.Id,
                        Url = img.ImageUrl,
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
            Species = petBase.Species ?? "",
            Breed = petBase.Breed,
            BirthDate = petBase.BirthDate,
            MicrochipNumber = petBase.MicrochipNumber,
            Notes = petBase.Notes,
            ClientNotes = petBase.ClientNotes,
            AgeMonths = age?.months,
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
                MicrochipNumber = p.MicrochipNumber,
                Notes = p.Notes,
                ClientNotes = p.ClientNotes
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
            MicrochipNumber = dto.MicrochipNumber,
            Notes = dto.Notes,
            ClientNotes = dto.ClientNotes
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
            MicrochipNumber = pet.MicrochipNumber,
            Notes = pet.Notes,
            ClientNotes = pet.ClientNotes
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
        pet.MicrochipNumber = dto.MicrochipNumber;
        pet.Notes = dto.Notes;
        pet.ClientNotes = dto.ClientNotes;

        // BirthDate öncelikli; yoksa AgeYears/AgeMonths'tan hesapla
        if (dto.BirthDate.HasValue)
        {
            pet.BirthDate = dto.BirthDate;
        }
        else if (dto.AgeYears.HasValue || dto.AgeMonths.HasValue)
        {
            var y = dto.AgeYears ?? 0;
            var m = dto.AgeMonths ?? 0;
            if (y < 0) y = 0;
            if (m < 0) m = 0;
            if (m > 11) m = 11;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            pet.BirthDate = today.AddYears(-y).AddMonths(-m);
        }

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
