namespace VetCrm.Api.Dtos;

public class LedgerEntryDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public bool IsIncome { get; set; }
    public string Category { get; set; } = null!;
    public string? Description { get; set; }

    public string? VisitPetName { get; set; }
    public string? VisitOwnerName { get; set; }

    public string? CreatedByName { get; set; }
}

public class LedgerRangeResponse
{
    public List<LedgerEntryDto> Items { get; set; } = new();
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Net => TotalIncome - TotalExpense;
}

public class CreateLedgerEntryDto
{
    public DateOnly Date { get; set; }      // input’ta sadece gün
    public decimal Amount { get; set; }
    public bool IsIncome { get; set; }
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
    public int? VisitId { get; set; }
    public int? CreatedByUserId { get; set; }  // şimdilik manuel 1 gönderebiliriz
}
