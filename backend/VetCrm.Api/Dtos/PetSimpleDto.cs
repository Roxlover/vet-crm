namespace VetCrm.Api.Dtos
{
    public class PetSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Species { get; set; } = "";
        public int? AgeYears { get; set; } 
        public int? AgeMonths { get; set; }  
        public string? Notes { get; set; } 
    }
}
