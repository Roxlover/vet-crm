namespace VetCrm.Api.Dtos
{
    public class CreateAppointmentRequest
    {
        public int OwnerId { get; set; }
        public List<int> PetIds { get; set; } = new();
        public DateTime ScheduledAt { get; set; }     
        public string? Purpose { get; set; }
        public int? DoctorId { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? VisitId { get; set; }
        public string? MicrochipNumber { get; set; }
        
        // Yeni alanlar: Randevu oluştururken finansal/klinik bilgi girişi için
        public decimal? AmountTl { get; set; }
        public decimal? CreditAmountTl { get; set; }
        public string? Procedures { get; set; }
        public string? Notes { get; set; }
        public decimal? PaidAmountTl { get; set; }
        public List<Microsoft.AspNetCore.Http.IFormFile>? Images { get; set; }
    }
}
