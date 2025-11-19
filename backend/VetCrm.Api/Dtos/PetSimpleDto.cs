namespace VetCrm.Api.Dtos
{
    public class PetSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Species { get; set; } = "";
        public int? AgeYears { get; set; }   // formdan gelen yaş
        public string? Notes { get; set; }   // “geçmişi” gibi düşünebiliriz
    }
}
