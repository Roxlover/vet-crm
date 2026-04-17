namespace VetCrm.Domain.Entities;

public class LedgerEntry
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public bool IsIncome { get; set; }

    public string? Category { get; set; }
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    // DB'de var: NOT NULL FK
    public int UserId { get; set; }
    public User? User { get; set; }

    public int? VisitId { get; set; }
    public Visit? Visit { get; set; }
}
