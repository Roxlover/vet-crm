namespace VetCrm.Domain.Entities;

public class Owner
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;
    public string PhoneE164 { get; set; } = null!; // 905xx... formatı
    public string? Email { get; set; }
    public string? Address { get; set; }

    public bool KvkkOptIn { get; set; } = true;

    public string? PasswordHash { get; set; }

    // Navigation
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    public ICollection<OwnerNote> Notes { get; set; } = new List<OwnerNote>();
}

