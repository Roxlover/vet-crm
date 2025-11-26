public class VisitDetailDto
{
    public int Id { get; set; }

    public int PetId { get; set; }       // 🔴 YENİ
    public int OwnerId { get; set; }     // 🔴 YENİ

    public string PetName { get; set; } = null!;
    public string OwnerName { get; set; } = null!;

    public DateTime PerformedAt { get; set; }
    public DateTime? NextDate { get; set; }
    public string? Purpose { get; set; }
    public string Procedures { get; set; } = string.Empty;
    public decimal? AmountTl { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? CreatedByName { get; set; }
    public string? CreatedByUsername { get; set; }
   public decimal? CreditAmountTl { get; set; }
}


