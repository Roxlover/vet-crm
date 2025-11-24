namespace VetCrm.Domain.Entities;

public class LedgerEntry
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }      // zorunlu
    public decimal Amount { get; set; }     // zorunlu
    public bool IsIncome { get; set; }      // true = gelir, false = gider

    public string? Category { get; set; }
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 🔴 ÖNEMLİ
}
