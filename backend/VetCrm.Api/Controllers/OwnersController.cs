using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public OwnersController(VetCrmDbContext db)
    {
        _db = db;
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
                PetCount = o.Pets.Count
            })
            .ToListAsync();

        return Ok(owners);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OwnerDto>> GetOwner(int id)
    {
        var owner = await _db.Owners
            .Include(o => o.Pets)
            .Where(o => o.Id == id)
            .Select(o => new OwnerDto
            {
                Id = o.Id,
                FullName = o.FullName,
                PhoneE164 = o.PhoneE164,
                Email = o.Email,
                Address = o.Address,
                KvkkOptIn = o.KvkkOptIn,
                PetCount = o.Pets.Count
            })
            .FirstOrDefaultAsync();

        if (owner is null)
            return NotFound();

        return Ok(owner);
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
            .Select(p => new Pet
            {
                Name = p.Name.Trim(),
                Species = p.Species,
                AgeYears = p.AgeYears,
                Notes = p.Notes
            })
            .ToList()
    };

    _db.Owners.Add(owner);
    await _db.SaveChangesAsync();

    // İstersen OwnerDto’ya mapleyip dönebilirsin
    var result = new OwnerDto
    {
        Id = owner.Id,
        FullName = owner.FullName,
        PhoneE164 = owner.PhoneE164,
        PetCount = owner.Pets.Count
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
    public async Task<ActionResult<PetSimpleDto>> AddPetToOwner(int ownerId, [FromBody] PetSimpleDto dto)
    {
        var owner = await _db.Owners.FindAsync(ownerId);
        if (owner == null)
            return NotFound();

        // Yaştan yaklaşık doğum tarihi hesapla (isteğe bağlı)
        DateOnly? birthDate = null;
        if (dto.AgeYears.HasValue && dto.AgeYears.Value > 0)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            birthDate = new DateOnly(today.Year - dto.AgeYears.Value, today.Month, today.Day);
        }

        var pet = new Pet
        {
            OwnerId = ownerId,
            Name = dto.Name,
            Species = dto.Species,
            Notes = dto.Notes,
            BirthDate = birthDate
        };

        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();

        dto.Id = pet.Id;
        return Ok(dto);
    }

}
