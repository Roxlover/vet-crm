public class VisitDetailDto
{
    public int Id { get; set; }
    public string PetName { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public DateTime PerformedAt { get; set; }
    public DateTime? NextDate { get; set; }
    public string? Purpose { get; set; }      // ❗ burada da var
    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }     // opsiyonel görsel
}

