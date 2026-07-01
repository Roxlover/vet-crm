using System.Text.Json.Serialization;

namespace VetCrm.Api.Dtos;

public class VisitUpdateDto
{
    public DateTime PerformedAt { get; set; }
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

    [JsonPropertyName("plans")]
    public List<VisitPlanCreateDto>? Plans { get; set; }

    [JsonPropertyName("nextVisits")]
    public List<VisitPlanCreateDto>? NextVisits
    {
        get => Plans;
        set => Plans = value;
    }
}
