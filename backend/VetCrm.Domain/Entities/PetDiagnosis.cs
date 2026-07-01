namespace VetCrm.Domain.Entities;

public enum DiagnosisStatus
{
    Aktif = 0,
    Iyilesti = 1,
    Kronik = 2
}

public class PetDiagnosis
{
    public int Id { get; set; }
    
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
    
    public int DiseaseId { get; set; }
    public Disease Disease { get; set; } = null!;
    
    public int? VisitId { get; set; }
    public Visit? Visit { get; set; }
    
    public DateTime DiagnosedDate { get; set; } = DateTime.UtcNow;
    
    public DiagnosisStatus Status { get; set; } = DiagnosisStatus.Aktif;
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
