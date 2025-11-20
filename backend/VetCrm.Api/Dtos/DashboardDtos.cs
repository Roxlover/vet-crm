namespace VetCrm.Api.Dtos;

public class ReminderItemDto
{
    public int Id { get; set; }              // Reminder Id
    public int VisitId { get; set; }         // <- YENİ
    public string PetName { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public DateOnly ReminderDate { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public string Procedures { get; set; } = "";
}


public class ReminderSummaryDto
{
    public int PendingToday { get; set; }
    public int PendingTomorrow { get; set; }
    public int Overdue { get; set; }
    public int Completed { get; set; }

    public List<ReminderItemDto> Upcoming { get; set; } = new();
}
