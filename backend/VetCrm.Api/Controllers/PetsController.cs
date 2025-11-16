using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public PetsController(VetCrmDbContext db)
    {
        _db = db;
    }

    // GET: api/pets?ownerId=1
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

    // GET: api/pets/5
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

    // POST: api/pets
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

        _db.Pets.Remove(pet);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
