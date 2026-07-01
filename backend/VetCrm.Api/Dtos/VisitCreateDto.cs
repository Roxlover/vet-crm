using System.Text.Json.Serialization;

namespace VetCrm.Api.Dtos;

public class VisitCreateDto
{
    public int PetId { get; set; }

    // frontend’ten boş gelirse backend "şimdi" kabul edebilir
    public DateTime? PerformedAt { get; set; }

    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }
    public string? Notes { get; set; }
    public string? ClientNotes { get; set; }
    public decimal? CreditAmountTl { get; set; }
    public decimal? CollectedAmountTl { get; set; }
    public string? Purpose { get; set; }

    public DateOnly? NextDate { get; set; }

    public string? MicrochipNumber { get; set; }
    
    // Disease Tracking
    public int? DiseaseId { get; set; }
    public string? DiagnosisStatus { get; set; }

    // Yeni isim: plans
    [JsonPropertyName("plans")]
    public List<VisitPlanCreateDto>? Plans { get; set; }

    // Eski/Frontend ismi: nextVisits  -> Plans'e map edilecek
    [JsonPropertyName("nextVisits")]
    public List<VisitPlanCreateDto>? NextVisits
    {
        get => Plans;
        set => Plans = value;
    }
    public int? Status { get; set; }
}
