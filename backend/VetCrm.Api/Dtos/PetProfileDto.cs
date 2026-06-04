namespace VetCrm.Api.Dtos;

public class PetProfileDto
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;
    public string? OwnerPhoneE164 { get; set; }
    public int? AgeMonths { get; set; }
    public string Name { get; set; } = null!;
    public string Species { get; set; } = null!;
    public string? Breed { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? MicrochipNumber { get; set; }
    public string? Notes { get; set; }

    public List<PetVisitDto> Visits { get; set; } = new();
}

public class PetVisitDto
{
    public int VisitId { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? Purpose { get; set; }
    public string? Procedures { get; set; }
    public decimal? AmountTl { get; set; }
    public decimal? CreditAmountTl { get; set; }
    public string? Notes { get; set; }

    public string? CreatedByUsername { get; set; }
    public string? CreatedByName { get; set; }

    public List<PetVisitImageDto> Images { get; set; } = new();
}

public class PetVisitImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
}
