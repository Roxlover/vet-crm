namespace VetCrm.Domain.Entities;

public class Pet
{
    public int Id { get; set; }

    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Species { get; set; }   // kedi/köpek
    public string? Breed { get; set; }     // ırk
    public DateOnly? BirthDate { get; set; }
    public string? MicrochipNumber { get; set; }
    public int? AgeYears { get; set; }
    public int? AgeMonths { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public string? ClientNotes { get; set; }

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<PetDiagnosis> Diagnoses { get; set; } = new List<PetDiagnosis>();
}
