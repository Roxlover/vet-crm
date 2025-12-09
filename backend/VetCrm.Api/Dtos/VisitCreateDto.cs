namespace VetCrm.Api.Dtos;

public class VisitCreateDto
{
    public int PetId { get; set; }

    // frontend’ten boş gelirse backend "şimdi" kabul edebilir
    public DateTime? PerformedAt { get; set; }

    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }
    public string? Notes { get; set; }

    public string? Purpose { get; set; }

    // Eski tekli yapı için (gerekirse)
    public DateOnly? NextDate { get; set; }

    public string? MicrochipNumber { get; set; }

    // 🔥 Çoklu "ne zaman gelecek" satırları
    public List<VisitPlanCreateDto> NextVisits { get; set; } = new();
}
