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
    private readonly Storage.IR2Storage _storage;

    public OwnersController(VetCrmDbContext db, Storage.IR2Storage storage)
    {
        _db = db;
        _storage = storage;
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
        .Include(o => o.Notes)
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
                    MicrochipNumber = p.MicrochipNumber,
                    AgeYears = age?.years,
                    AgeMonths = age?.months,
                    TotalAmount = _db.Visits.Where(v => v.PetId == p.Id).Sum(v => v.AmountTl) ?? 0,
                    TotalCredit = _db.Visits.Where(v => v.PetId == p.Id).Sum(v => v.CreditAmountTl) ?? 0
                };
            })
            .ToList(),
        Notes = ownerEntity.Notes
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new OwnerNoteDto
            {
                Id = n.Id,
                Note = n.Note,
                ImageUrl = n.ImageUrl,
                CreatedAt = n.CreatedAt
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
            PasswordHash = !string.IsNullOrWhiteSpace(dto.Password) ? BCrypt.Net.BCrypt.HashPassword(dto.Password) : null,
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

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            owner.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

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

[HttpPost("{ownerId}/notes")]
public async Task<ActionResult> AddNoteToOwner(int ownerId, [FromForm] AddOwnerNoteRequest request)
{
    var owner = await _db.Owners.FindAsync(ownerId);
    if (owner == null)
        return NotFound();

    if (string.IsNullOrWhiteSpace(request.Note) && request.Image == null)
        return BadRequest("Not veya görsel eklenmelidir.");

    string? imageUrl = null;
    if (request.Image != null)
    {
        await using var stream = request.Image.OpenReadStream();
        imageUrl = await _storage.UploadOwnerNoteImageAsync(
            ownerId: ownerId,
            stream: stream,
            contentType: request.Image.ContentType ?? "application/octet-stream"
        );
    }

    var note = new OwnerNote
    {
        OwnerId = ownerId,
        Note = request.Note?.Trim(),
        ImageUrl = imageUrl,
        CreatedAt = DateTime.UtcNow
    };

    _db.OwnerNotes.Add(note);
    await _db.SaveChangesAsync();

    return Ok(new OwnerNoteDto
    {
        Id = note.Id,
        Note = note.Note,
        ImageUrl = note.ImageUrl,
        CreatedAt = note.CreatedAt
    });
}

[HttpPut("{ownerId}/notes/{noteId}")]
public async Task<ActionResult> UpdateNote(int ownerId, int noteId, [FromBody] AddOwnerNoteRequest request)
{
    var note = await _db.OwnerNotes.FirstOrDefaultAsync(n => n.Id == noteId && n.OwnerId == ownerId);
    if (note == null)
        return NotFound();

    if (string.IsNullOrWhiteSpace(request.Note) && string.IsNullOrWhiteSpace(note.ImageUrl))
        return BadRequest("Not veya görsel eklenmelidir.");

    note.Note = request.Note?.Trim();
    await _db.SaveChangesAsync();

    return NoContent();
}

[HttpDelete("{ownerId}/notes/{noteId}")]
public async Task<ActionResult> DeleteNote(int ownerId, int noteId)
{
    var note = await _db.OwnerNotes.FirstOrDefaultAsync(n => n.Id == noteId && n.OwnerId == ownerId);
    if (note == null)
        return NotFound();

    _db.OwnerNotes.Remove(note);
    await _db.SaveChangesAsync();

    return NoContent();
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
    public async Task<ActionResult<List<OwnerDto>>> SearchOwners([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new List<OwnerDto>());

        var queryLower = query.Trim().ToLower();

        // 1. Veritabanından uyan kayıtları al (Fazla alıyoruz ki sıralamada kayıp olmasın)
        var ownersDb = await _db.Owners
            .Include(o => o.Pets)
            .Where(o =>
                o.FullName.ToLower().Contains(queryLower) ||
                o.PhoneE164.Contains(queryLower))
            .Take(50)
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

        // 2. Bellekte sıralamayı yap: Sadece aranan harfle başlayanlar her zaman en üstte
        var owners = ownersDb
            .OrderByDescending(o => o.FullName.ToLower().StartsWith(queryLower))
            .ThenBy(o => o.FullName)
            .Take(20)
            .ToList();

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
