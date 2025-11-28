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

public class LedgerSummaryDto
{
    // Toplam yazılan işlem tutarı (fatura/fiş gibi düşün)
    public decimal TotalAmount { get; set; }

    // Tahsil edilen (nakit/kart)
    public decimal TotalCollected { get; set; }

    // Halen veresiye kalan
    public decimal TotalCredit { get; set; }

    // Ziyaret adedi
    public int VisitCount { get; set; }
}

public class LedgerItemDto
{
    public int VisitId { get; set; }
    public DateTime PerformedAt { get; set; }

    public string PetName { get; set; } = null!;
    public string OwnerName { get; set; } = null!;
    public string? OwnerPhoneE164 { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal CreditAmount { get; set; }

    public string? CreatedByUsername { get; set; }
    public string? CreatedByName { get; set; }
}