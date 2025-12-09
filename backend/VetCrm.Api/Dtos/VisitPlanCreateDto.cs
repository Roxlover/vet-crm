namespace VetCrm.Api.Dtos;

public class VisitPlanCreateDto
{
    public DateOnly Date { get; set; }
    public string? Purpose { get; set; }
    public int? DoctorId { get; set; }
}
