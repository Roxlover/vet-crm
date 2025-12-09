namespace VetCrm.Domain.Entities;

public class VisitPlan
{
    public int Id { get; set; }

    public int VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public DateOnly Date { get; set; }          // Ne zaman gelecek?
    public string? Purpose { get; set; }        // Ne için gelecek?
    public int? DoctorId { get; set; }          // İstersen buraya da doktor bağlarız
    public User? Doctor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
