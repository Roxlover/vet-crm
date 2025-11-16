namespace VetCrm.Api.Dtos;

public class OwnerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneE164 { get; set; } = null!;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool KvkkOptIn { get; set; }

    public int PetCount { get; set; }
}
