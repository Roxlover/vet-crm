namespace VetCrm.Api.Dtos;

public class VisitUpdateDto
{
    public DateTime PerformedAt { get; set; }
    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }
    public string? Notes { get; set; }

    public DateOnly? NextDate { get; set; }
}
