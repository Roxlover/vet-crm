namespace VetCrm.Api.Dtos;

public class UpdateVisitCollectedDto
{
    public decimal? CollectedAmountTl { get; set; }
    public string? Note { get; set; }   // istersen sebep/neden için
}
