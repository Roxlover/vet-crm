namespace VetCrm.Domain.Entities;

public class Pet
{
    public int Id { get; set; }

    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Species { get; set; }   // kedi/köpek
    public string? Breed { get; set; }     // ırk
    public DateOnly? BirthDate { get; set; }

    public string? Notes { get; set; }

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
