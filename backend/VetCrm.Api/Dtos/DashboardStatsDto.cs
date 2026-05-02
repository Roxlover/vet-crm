namespace VetCrm.Api.Dtos;

public class DashboardStatsDto
{
    public int ActivePetsCount { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int TodayAppointmentsCount { get; set; }
    public int PendingRemindersCount { get; set; }
    public List<WeeklyActivityDto> WeeklyActivity { get; set; } = new();
}

public class WeeklyActivityDto
{
    public string Date { get; set; } = null!;
    public int VisitCount { get; set; }
}
