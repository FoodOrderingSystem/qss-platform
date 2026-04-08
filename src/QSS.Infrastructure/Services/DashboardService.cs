using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QSS.Application.DTOs;
using QSS.Domain.Enums;
using QSS.Infrastructure.Data;

namespace QSS.Infrastructure.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ApplicationDbContext db, ILogger<DashboardService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        try
        {
            return await BuildDashboardAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building dashboard data. Returning empty dashboard.");
            return new DashboardDto();
        }
    }

    private async Task<DashboardDto> BuildDashboardAsync()
    {
        var now = DateTime.UtcNow;

        // Use SQL-side aggregates instead of loading full entities into memory.
        // This avoids potential EF Core Include-chain translation issues, circular
        // references, and memory exhaustion on large datasets.
        var total      = await _db.Tasks.CountAsync();
        var completed  = await _db.Tasks.CountAsync(t => t.Status == QssTaskStatus.Completed);
        var overdue    = await _db.Tasks.CountAsync(t =>
            t.Status == QssTaskStatus.Overdue ||
            (t.Status != QssTaskStatus.Completed && t.DueDate.HasValue && t.DueDate < now));
        var open       = await _db.Tasks.CountAsync(t => t.Status == QssTaskStatus.Open);
        var inProgress = await _db.Tasks.CountAsync(t => t.Status == QssTaskStatus.InProgress);

        var qssScore = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0.0;

        var users    = await _db.Users.CountAsync();
        var lowStock = await _db.Materials.CountAsync(m => m.StockQuantity <= m.MinimumStockLevel);
        var expMeds  = await _db.Medications.CountAsync(
            m => m.ExpirationDate.HasValue && m.ExpirationDate < now.AddDays(30));

        var alerts = BuildAlerts(overdue, lowStock, expMeds);

        // Top performers: aggregate at DB level, then resolve names in a second small query.
        var perfGroups = await _db.Tasks
            .Where(t => t.AssigneeId != null && t.AssigneeId != "")
            .GroupBy(t => t.AssigneeId)
            .Select(g => new
            {
                AssigneeId     = g.Key,
                TotalAssigned  = g.Count(),
                TotalCompleted = g.Count(t => t.Status == QssTaskStatus.Completed)
            })
            .ToListAsync();

        var topGroups = perfGroups
            .OrderByDescending(g => g.TotalAssigned > 0
                ? (double)g.TotalCompleted / g.TotalAssigned
                : 0)
            .Take(5)
            .ToList();

        Dictionary<string, string> userNames = new();
        if (topGroups.Count > 0)
        {
            var ids = topGroups.Select(p => p.AssigneeId!).ToList();
            userNames = await _db.Users
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
                .ToDictionaryAsync(u => u.Id, u => u.Name);
        }

        var perf = topGroups.Select(g => new EmployeePerformanceDto
        {
            UserId         = g.AssigneeId ?? "",
            UserName       = userNames.TryGetValue(g.AssigneeId ?? "", out var n) ? n : "Unknown",
            TotalAssigned  = g.TotalAssigned,
            TotalCompleted = g.TotalCompleted,
            CompletionRate = g.TotalAssigned > 0
                ? Math.Round((double)g.TotalCompleted / g.TotalAssigned * 100, 1)
                : 0
        }).ToList();

        // Recent tasks: server-side projection avoids loading full entity graphs
        // and prevents circular-reference serialisation issues.
        var recentTasks = await _db.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new TaskDto
            {
                Id           = t.Id,
                Name         = t.Name,
                Status       = t.Status.ToString(),
                DueDate      = t.DueDate,
                AssigneeName = t.Assignee != null
                    ? t.Assignee.FirstName + " " + t.Assignee.LastName
                    : null,
                ProcessName  = t.Process != null ? t.Process.Name : null,
                CreatedAt    = t.CreatedAt
            })
            .ToListAsync();

        return new DashboardDto
        {
            QssScore            = qssScore,
            TotalTasks          = total,
            CompletedTasks      = completed,
            OverdueTasks        = overdue,
            OpenTasks           = open,
            InProgressTasks     = inProgress,
            TotalUsers          = users,
            LowStockMaterials   = lowStock,
            ExpiringMedications = expMeds,
            Alerts              = alerts,
            RecentTasks         = recentTasks,
            TopPerformers       = perf
        };
    }

    private static List<AlertDto> BuildAlerts(int overdue, int lowStock, int expMeds)
    {
        var alerts = new List<AlertDto>();
        if (overdue > 0)
            alerts.Add(new AlertDto
            {
                Type        = "danger",
                Message     = $"{overdue} task(s) are overdue.",
                ResourceUrl = "/tasks?status=overdue"
            });
        if (lowStock > 0)
            alerts.Add(new AlertDto
            {
                Type        = "warning",
                Message     = $"{lowStock} material(s) are low on stock.",
                ResourceUrl = "/materials"
            });
        if (expMeds > 0)
            alerts.Add(new AlertDto
            {
                Type        = "warning",
                Message     = $"{expMeds} medication(s) expire within 30 days.",
                ResourceUrl = "/medications"
            });
        return alerts;
    }
}
