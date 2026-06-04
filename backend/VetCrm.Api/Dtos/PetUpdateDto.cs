namespace VetCrm.Api.Dtos;

public class PetUpdateDto
{
    public string Name { get; set; } = null!;
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? MicrochipNumber { get; set; }
    public string? Notes { get; set; }
    public int? AgeYears { get; set; }
    public int? AgeMonths { get; set; }
}
