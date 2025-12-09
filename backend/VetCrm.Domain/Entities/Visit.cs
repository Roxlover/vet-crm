namespace VetCrm.Domain.Entities;

public class Visit
{
    public int Id { get; set; }

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    public DateTime PerformedAt { get; set; }

    // Legacy tekli alan (dashboard + upcoming için hâlâ işimize yarayacak)
    public DateOnly? NextDate { get; set; }

    public string? Purpose { get; set; }
    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }
    public string? Notes { get; set; }

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 🔹 Yeni: çoklu "ne zaman gelecek" planları
    public ICollection<VisitPlan> Plans { get; set; } = new List<VisitPlan>();
}
