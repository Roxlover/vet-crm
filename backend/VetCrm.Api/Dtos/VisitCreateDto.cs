namespace VetCrm.Api.Dtos;

public class VisitCreateDto
{
    public int PetId { get; set; }

    // frontend’ten boş gelirse backend "şimdi" kabul edebilir
    public DateTime? PerformedAt { get; set; }

    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }
    public string? Notes { get; set; }

    public DateOnly? NextDate { get; set; }  // defterdeki "ne zaman gelecek"
}
