namespace VetCrm.Api.Dtos;

public class VisitPlanDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Purpose { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
}
