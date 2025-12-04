namespace VetCrm.Api.Dtos;

public class VisitImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

}
