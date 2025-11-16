namespace VetCrm.Api.Dtos;

public class PetDto
{
    public int Id { get; set; }

    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Species { get; set; }  
    public string? Breed { get; set; }   
    public DateOnly? BirthDate { get; set; }

    public string? Notes { get; set; }
}
