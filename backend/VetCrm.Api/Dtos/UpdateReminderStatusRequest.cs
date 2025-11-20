namespace VetCrm.Api.Dtos;

public class UpdateReminderStatusRequest
{
    public bool Completed { get; set; }
    public bool MarkAsOverdue { get; set; } = false;
}
