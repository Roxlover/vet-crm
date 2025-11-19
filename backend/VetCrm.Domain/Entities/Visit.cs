namespace VetCrm.Domain.Entities;

public class Visit
{
    public int Id { get; set; }

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    public DateTime PerformedAt { get; set; }  
    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }     
    public string? Notes { get; set; }

    public DateOnly? NextDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? Purpose { get; set; }  // Ne için gelecek

    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
