namespace VetCrm.Api.Dtos;

public class VisitDto
{
    public int Id { get; set; }

    public int PetId { get; set; }
    public string PetName { get; set; } = null!;

    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;

    public DateTime PerformedAt { get; set; }      // işlem tarihi
    public string? Procedures { get; set; }        // neler uygulandı
    public decimal? AmountTl { get; set; }         // ne kadar aldım
    public string? Notes { get; set; }
    public string? Purpose { get; set; }
    public DateOnly? NextDate { get; set; }        // ne zaman gelecek
    public decimal? CreditAmountTl { get; set; }
    public string? CreatedByUsername { get; set; }
    public string? CreatedByName { get; set; }
    public int? CreatedByUserId { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? ImageUrl { get; set; }
}
