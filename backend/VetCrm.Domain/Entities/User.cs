namespace VetCrm.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    // 🔹 YENİ: Kullanıcı adı ile login
    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = "Doctor";

    public string PasswordHash { get; set; } = null!;

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
