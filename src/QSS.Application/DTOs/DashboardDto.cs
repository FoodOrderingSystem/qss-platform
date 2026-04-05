namespace QSS.Application.DTOs;

public class DashboardDto
{
    public double QssScore { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int OpenTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int TotalUsers { get; set; }
    public int LowStockMaterials { get; set; }
    public int ExpiringMedications { get; set; }
    public List<TaskDto> RecentTasks { get; set; } = new();
    public List<AlertDto> Alerts { get; set; } = new();
    public List<EmployeePerformanceDto> TopPerformers { get; set; } = new();
}

public class AlertDto
{
    public string Type { get; set; } = string.Empty; // danger, warning, info
    public string Message { get; set; } = string.Empty;
    public string? ResourceUrl { get; set; }
}

public class EmployeePerformanceDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int TotalCompleted { get; set; }
    public double CompletionRate { get; set; }
}
