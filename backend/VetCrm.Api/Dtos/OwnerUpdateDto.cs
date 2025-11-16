namespace VetCrm.Api.Dtos;

public class OwnerUpdateDto
{
    public string FullName { get; set; } = null!;
    public string PhoneE164 { get; set; } = null!;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool KvkkOptIn { get; set; }
}
