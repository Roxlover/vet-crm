namespace VetCrm.Domain.Entities;

public enum DiseaseCategory
{
    Enfeksiyoz = 0,
    Paraziter = 1,
    Kronik = 2,
    Genetik = 3,
    Diger = 4
}

public class Disease
{
    public int Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    public DiseaseCategory Category { get; set; }
    
    public string? Species { get; set; } // e.g. "Kedi", "Köpek", "Tümü"
    
    public string? Description { get; set; }
    
    public bool IsContagious { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
