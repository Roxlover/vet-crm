namespace VetCrm.Api.Dtos;

public class ReminderDashboardDto
{
    public int Id { get; set; }
    public string OwnerName { get; set; } = null!;
    public string OwnerPhone { get; set; } = null!;
    public string PetName { get; set; } = null!;
    public DateOnly DueDate { get; set; }
    public string? Procedures { get; set; }
    public bool IsCompleted { get; set; }
    public string? VisitImageUrl { get; set; }
}