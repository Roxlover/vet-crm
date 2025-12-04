namespace VetCrm.Api.Dtos;

public class DashboardVisitDetailDto
{
    public int Id { get; set; }

    public int PetId { get; set; }
    public string PetName { get; set; } = null!;

    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;

    public DateTime PerformedAt { get; set; }
    public DateOnly? NextDate { get; set; }

    public string? Purpose { get; set; }
    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? CreditAmountTl { get; set; }

    // Doktor bilgisi
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }

    // Kaydı ekleyen kullanıcı
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUsername { get; set; }
    public string? CreatedByName { get; set; }
    public List<VisitImageDto> Images { get; set; } = new();

}
