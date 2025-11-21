public class CalendarAppointmentDto
{
    public int VisitId { get; set; }

    public DateTime ScheduledAt { get; set; } // PerformedAt
    public DateOnly? NextDate { get; set; }

    public string PetName { get; set; } = null!;
    public string OwnerName { get; set; } = null!;
    public string? Purpose { get; set; }

    public string? DoctorName { get; set; }
}
