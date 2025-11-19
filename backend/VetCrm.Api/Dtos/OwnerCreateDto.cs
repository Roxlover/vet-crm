namespace VetCrm.Api.Dtos;

public class OwnerPetCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Species { get; set; }
    public int? AgeYears { get; set; }
    public string? Notes { get; set; }
}

public class OwnerCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneE164 { get; set; } = string.Empty;
    public bool KvkkOptIn { get; set; }

    public List<OwnerPetCreateDto> Pets { get; set; } = new();
}
