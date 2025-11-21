namespace VetCrm.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;

    // Role: "Doctor", "Assistant", "Admin" gibi
    public string Role { get; set; } = "Doctor";

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
