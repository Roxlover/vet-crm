namespace VetCrm.Api.Dtos;

public class UpcomingVisitDto
{
    public int VisitId { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = null!;

    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;
    public string OwnerPhoneE164 { get; set; } = null!;

    public DateOnly VisitDate { get; set; }       // NextDate
    public string? Procedures { get; set; }       // ne yapılacak
    public bool WhatsAppSent { get; set; }        // ileride kullanacağız
}
