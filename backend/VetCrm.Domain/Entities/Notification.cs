namespace VetCrm.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Type { get; set; } = "AppointmentCreated";
    public string Message { get; set; } = null!;

    public int? VisitId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
