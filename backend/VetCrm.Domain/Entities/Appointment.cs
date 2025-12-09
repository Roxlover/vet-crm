namespace VetCrm.Domain.Entities;

public class Appointment
{
    public int Id { get; set; }

    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }

    public string? Purpose { get; set; }

    public int? DoctorId { get; set; }
    public User? Doctor { get; set; }

    public int? VisitId { get; set; }
    public Visit? Visit { get; set; }
}
