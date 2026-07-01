namespace VetCrm.Domain.Entities;

public class Visit
{
    public int Id { get; set; }

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
    public decimal? CollectedAmountTl { get; set; }
    public DateTime PerformedAt { get; set; }

    public DateOnly? NextDate { get; set; }

    public string? Purpose { get; set; }
    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }
    public string? Notes { get; set; }
    public string? ClientNotes { get; set; }

    public string? ImageUrl { get; set; }
    public ICollection<VisitImage> Images { get; set; } = new List<VisitImage>();

    public int? DoctorId { get; set; }
    public User? Doctor { get; set; }

    public int? CreatedByUserId { get; set; }
    public string? CreatedByUsername { get; set; }
    public string? CreatedByName { get; set; }
    public User? CreatedByUser { get; set; }

    public decimal? CreditAmountTl { get; set; }
    public string? MicrochipNumber { get; set; }

    public enum VisitStatus
{
    Pending = 0,     // default
    Completed = 1,   // Yapıldı
    Missed = 2       // Yapılmadı (gecikene düşür)
}

public VisitStatus Status { get; set; } = VisitStatus.Pending;
public DateTime? StatusUpdatedAt { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<VisitPlan> Plans { get; set; } = new List<VisitPlan>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<PetDiagnosis> Diagnoses { get; set; } = new List<PetDiagnosis>();
}
