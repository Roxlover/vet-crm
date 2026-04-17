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
public class OwnersController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public OwnersController(VetCrmDbContext db)
    {
        _db = db;
    }
    private static DateOnly? BirthDateFromAge(int? years, int? months)
{
    if (years is null && months is null) return null;

    var y = years ?? 0;
    var m = months ?? 0;

    if (y < 0) y = 0;
    if (m < 0) m = 0;
    if (m > 11) m = 11;

    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    // DateOnly'de AddMonths/AddYears var
    var bd = today.AddYears(-y).AddMonths(-m);
    return bd;
}

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OwnerDto>>> GetOwners()
    {
        var owners = await _db.Owners
            .Include(o => o.Pets)
            .Select(o => new OwnerDto
            {
                Id = o.Id,
                FullName = o.FullName,
                PhoneE164 = o.PhoneE164,
                Email = o.Email,
                Address = o.Address,
                KvkkOptIn = o.KvkkOptIn,
                PetCount = o.Pets.Count(p => p.IsActive)
            })
            .ToListAsync();

        return Ok(owners);
    }

    [HttpGet("{id:int}")]
public async Task<ActionResult<OwnerDto>> GetOwner(int id)
{
    var ownerEntity = await _db.Owners
        .Include(o => o.Pets)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (ownerEntity is null)
        return NotFound();

    var dto = new OwnerDto
    {
        Id = ownerEntity.Id,
        FullName = ownerEntity.FullName,
        PhoneE164 = ownerEntity.PhoneE164,
        Email = ownerEntity.Email,
        Address = ownerEntity.Address,
        KvkkOptIn = ownerEntity.KvkkOptIn,
        PetCount = ownerEntity.Pets.Count(p => p.IsActive),
        Pets = ownerEntity.Pets
            .OrderBy(p => p.Name)
            .Select(p =>
            {
                var age = OwnerDto.CalcAge(p.BirthDate);

                return new OwnerPetFullDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Species = p.Species,
                    Breed = p.Breed,
                    BirthDate = p.BirthDate,
                    AgeYears = age?.years,
                    AgeMonths = age?.months
                };
            })
            .ToList()
    };

    return Ok(dto);
}
 
    [HttpPost]
    public async Task<ActionResult<OwnerDto>> CreateOwner([FromBody] OwnerCreateDto dto)
    {
        var owner = new Owner
        {
            FullName = dto.FullName,
            PhoneE164 = dto.PhoneE164,
            KvkkOptIn = dto.KvkkOptIn,
            Pets = dto.Pets
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))

                .Select(p =>
{
    var derivedBirthDate = BirthDateFromAge(p.AgeYears, p.AgeMonths);

    return new Pet
    {
        Name = p.Name.Trim(),
        Species = p.Species,
        // Artık response yaş hesapladığı için BirthDate kritik:
        BirthDate = derivedBirthDate,
        // AgeYears DB’de var; istersen tut, istersen null bırak.
        // Tutmak istiyorsan:
        AgeYears = p.AgeYears,
        Notes = p.Notes,
        IsActive = true
    };
})

                .ToList()
        };

        _db.Owners.Add(owner);
        await _db.SaveChangesAsync();

        var result = new OwnerDto
        {
            Id = owner.Id,
            FullName = owner.FullName,
            PhoneE164 = owner.PhoneE164,
            PetCount = owner.Pets.Count(p => p.IsActive)
        };

        return CreatedAtAction(nameof(GetOwner), new { id = owner.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOwner(int id, [FromBody] OwnerUpdateDto dto)
    {
        var owner = await _db.Owners.FindAsync(id);
        if (owner is null)
            return NotFound();

        owner.FullName = dto.FullName;
        owner.PhoneE164 = dto.PhoneE164;
        owner.Email = dto.Email;
        owner.Address = dto.Address;
        owner.KvkkOptIn = dto.KvkkOptIn;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOwner(int id)
    {
        var owner = await _db.Owners.FindAsync(id);
        if (owner is null)
            return NotFound();

        _db.Owners.Remove(owner);
        await _db.SaveChangesAsync();

        return NoContent();
    }

 [HttpPost("{ownerId}/pets")]
 public async Task<ActionResult> AddPetToOwner(int ownerId, [FromBody] PetCreateDto dto)
 {
    var owner = await _db.Owners.FindAsync(ownerId);
    if (owner == null)
        return NotFound();

    if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest("Pet adı zorunludur.");

   var derivedBirthDate = dto.BirthDate ?? BirthDateFromAge(dto.AgeYears, dto.AgeMonths);

var pet = new Pet
{
    OwnerId = ownerId,
    Name = dto.Name.Trim(),
    Species = dto.Species,
    Breed = dto.Breed,
    BirthDate = derivedBirthDate,
    AgeYears = dto.AgeYears, // istersen tut
    Notes = dto.Notes,
    IsActive = true
};

    _db.Pets.Add(pet);
    await _db.SaveChangesAsync();

    return Ok(new
    {
        id = pet.Id,
        name = pet.Name,
        species = pet.Species,
        breed = pet.Breed,
        birthDate = pet.BirthDate,
        notes = pet.Notes
    });
}
 

    [HttpGet("{id:int}/pets")]
    public async Task<ActionResult<List<OwnerPetDto>>> GetOwnerPets(int id)
    {
        var pets = await _db.Pets
            .Where(p => p.OwnerId == id && p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new OwnerPetDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToListAsync();

        return Ok(pets);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<OwnerSearchDto>>> SearchOwners([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new List<OwnerSearchDto>());

        query = query.Trim();

        var owners = await _db.Owners
            .Where(o =>
                o.FullName.ToLower().Contains(query.ToLower()) ||
                o.PhoneE164.Contains(query))
            .OrderBy(o => o.FullName)
            .Take(20)
            .Select(o => new OwnerSearchDto
            {
                Id = o.Id,
                FullName = o.FullName,
                Phone = o.PhoneE164
            })
            .ToListAsync();

        return Ok(owners);
    }
}

public class OwnerPetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class OwnerSearchDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
}
