using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DiseasesController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public DiseasesController(VetCrmDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> GetDiseases([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? category = null, [FromQuery] string? species = null)
    {
        var query = _db.Diseases.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<DiseaseCategory>(category, true, out var catEnum))
        {
            query = query.Where(d => d.Category == catEnum);
        }

        if (!string.IsNullOrWhiteSpace(species))
        {
            query = query.Where(d => d.Species != null && d.Species.ToLower().Contains(species.ToLower()));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new
            {
                d.Id,
                d.Name,
                Category = d.Category.ToString(),
                d.Species,
                d.Description,
                d.IsContagious
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetDisease(int id)
    {
        var d = await _db.Diseases.FindAsync(id);
        if (d == null) return NotFound();

        return Ok(new
        {
            d.Id,
            d.Name,
            Category = d.Category.ToString(),
            d.Species,
            d.Description,
            d.IsContagious
        });
    }

    [HttpPost]
    [Authorize(Policy = "BullBossOnly")]
    public async Task<ActionResult> CreateDisease([FromBody] DiseaseDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("İsim zorunludur.");
        
        if (!Enum.TryParse<DiseaseCategory>(dto.Category, true, out var catEnum))
            return BadRequest("Geçersiz kategori.");

        var disease = new Disease
        {
            Name = dto.Name,
            Category = catEnum,
            Species = dto.Species,
            Description = dto.Description,
            IsContagious = dto.IsContagious
        };

        _db.Diseases.Add(disease);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDisease), new { id = disease.Id }, new { disease.Id });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "BullBossOnly")]
    public async Task<ActionResult> UpdateDisease(int id, [FromBody] DiseaseDto dto)
    {
        var disease = await _db.Diseases.FindAsync(id);
        if (disease == null) return NotFound();

        if (!Enum.TryParse<DiseaseCategory>(dto.Category, true, out var catEnum))
            return BadRequest("Geçersiz kategori.");

        disease.Name = dto.Name;
        disease.Category = catEnum;
        disease.Species = dto.Species;
        disease.Description = dto.Description;
        disease.IsContagious = dto.IsContagious;
        disease.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "BullBossOnly")]
    public async Task<ActionResult> DeleteDisease(int id)
    {
        var disease = await _db.Diseases.FindAsync(id);
        if (disease == null) return NotFound();

        _db.Diseases.Remove(disease);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class DiseaseDto
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Species { get; set; }
    public string? Description { get; set; }
    public bool IsContagious { get; set; }
}
