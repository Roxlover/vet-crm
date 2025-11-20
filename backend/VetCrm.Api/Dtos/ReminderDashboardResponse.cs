namespace VetCrm.Api.Dtos;

public class ReminderDashboardResponse
{
    public List<ReminderDashboardDto> Today { get; set; } = new();
    public List<ReminderDashboardDto> Tomorrow { get; set; } = new();
    public List<ReminderDashboardDto> Overdue { get; set; } = new();
    public List<ReminderDashboardDto> Done { get; set; } = new();
}