namespace VetCrm.Api.Dtos
{
    public class CreateAppointmentRequest
    {
        public int OwnerId { get; set; }
        public List<int> PetIds { get; set; } = new();
        public DateTime ScheduledAt { get; set; }      // tarih + saat
        public string? Purpose { get; set; }
        public int? DoctorId { get; set; }
        public int? CreatedByUserId { get; set; }
    }
}
