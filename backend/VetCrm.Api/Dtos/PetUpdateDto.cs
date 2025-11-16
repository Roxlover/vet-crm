namespace VetCrm.Api.Dtos;

public class PetUpdateDto
{
    public string Name { get; set; } = null!;
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public DateOnly? BirthDate { get; set; }

    public string? Notes { get; set; }
}
