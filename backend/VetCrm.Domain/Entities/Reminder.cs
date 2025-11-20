namespace VetCrm.Domain.Entities;

public enum ReminderStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}

public class Reminder
{
    public int Id { get; set; }

    public int VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public DateOnly DueDate { get; set; }      // ne zaman hatırlatılacak (1 gün öncesi)
    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;

    public string? ErrorMessage { get; set; }  // WA hata logu için
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
}
