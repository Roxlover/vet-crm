using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PetDiagnosesController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public PetDiagnosesController(VetCrmDbContext db)
    {
        _db = db;
    }

    [HttpGet("/api/pets/{petId:int}/diagnoses")]
    public async Task<ActionResult> GetPetDiagnoses(int petId)
    {
        var diagnoses = await _db.PetDiagnoses
            .Include(pd => pd.Disease)
            .Include(pd => pd.Visit)
            .Where(pd => pd.PetId == petId)
            .OrderByDescending(pd => pd.DiagnosedDate)
            .Select(pd => new
            {
                pd.Id,
                pd.PetId,
                pd.DiseaseId,
                DiseaseName = pd.Disease.Name,
                DiseaseCategory = pd.Disease.Category.ToString(),
                pd.VisitId,
                pd.DiagnosedDate,
                Status = pd.Status.ToString(),
                pd.Notes,
                PerformedAt = pd.Visit != null ? pd.Visit.PerformedAt : (DateTime?)null
            })
            .ToListAsync();

        return Ok(diagnoses);
    }

    [HttpPost("/api/pets/{petId:int}/diagnoses")]
    public async Task<ActionResult> AddPetDiagnosis(int petId, [FromBody] PetDiagnosisCreateDto dto)
    {
        var pet = await _db.Pets.FindAsync(petId);
        if (pet == null) return NotFound("Hasta bulunamadı.");

        var disease = await _db.Diseases.FindAsync(dto.DiseaseId);
        if (disease == null) return NotFound("Hastalık bulunamadı.");

        if (!Enum.TryParse<DiagnosisStatus>(dto.Status, true, out var statusEnum))
            statusEnum = DiagnosisStatus.Aktif;

        var diagnosis = new PetDiagnosis
        {
            PetId = petId,
            DiseaseId = dto.DiseaseId,
            VisitId = dto.VisitId,
            Status = statusEnum,
            Notes = dto.Notes,
            DiagnosedDate = dto.DiagnosedDate ?? DateTime.UtcNow
        };

        _db.PetDiagnoses.Add(diagnosis);
        await _db.SaveChangesAsync();

        return Ok(new { diagnosis.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateDiagnosisStatus(int id, [FromBody] PetDiagnosisUpdateDto dto)
    {
        var diagnosis = await _db.PetDiagnoses.FindAsync(id);
        if (diagnosis == null) return NotFound();

        if (Enum.TryParse<DiagnosisStatus>(dto.Status, true, out var statusEnum))
        {
            diagnosis.Status = statusEnum;
            diagnosis.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }
}

public class PetDiagnosisCreateDto
{
    public int DiseaseId { get; set; }
    public int? VisitId { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? DiagnosedDate { get; set; }
}

public class PetDiagnosisUpdateDto
{
    public string Status { get; set; } = null!;
}
