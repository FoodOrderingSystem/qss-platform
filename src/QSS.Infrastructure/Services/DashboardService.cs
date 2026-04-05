using Microsoft.EntityFrameworkCore;
using QSS.Application.DTOs;
using QSS.Domain.Enums;
using QSS.Infrastructure.Data;

namespace QSS.Infrastructure.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db) => _db = db;

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var tasks = await _db.Tasks.Include(t => t.Process).Include(t => t.Assignee).ToListAsync();
        var total = tasks.Count;
        var completed = tasks.Count(t => t.Status == QssTaskStatus.Completed);
        var overdue = tasks.Count(t => t.Status == QssTaskStatus.Overdue ||
                                       (t.Status != QssTaskStatus.Completed && t.DueDate.HasValue && t.DueDate < DateTime.UtcNow));
        var open = tasks.Count(t => t.Status == QssTaskStatus.Open);
        var inProgress = tasks.Count(t => t.Status == QssTaskStatus.InProgress);

        var qssScore = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0;

        var users = await _db.Users.CountAsync();

        var materials = await _db.Materials.ToListAsync();
        var lowStock = materials.Count(m => m.StockQuantity <= m.MinimumStockLevel);

        var meds = await _db.Medications.ToListAsync();
        var expMeds = meds.Count(m => m.ExpirationDate.HasValue && m.ExpirationDate < DateTime.UtcNow.AddDays(30));

        var alerts = new List<AlertDto>();
        if (overdue > 0) alerts.Add(new AlertDto { Type = "danger", Message = $"{overdue} task(s) are overdue.", ResourceUrl = "/tasks?status=overdue" });
        if (lowStock > 0) alerts.Add(new AlertDto { Type = "warning", Message = $"{lowStock} material(s) are low on stock.", ResourceUrl = "/materials" });
        if (expMeds > 0) alerts.Add(new AlertDto { Type = "warning", Message = $"{expMeds} medication(s) expire within 30 days.", ResourceUrl = "/medications" });

        var perf = tasks
            .Where(t => t.AssigneeId != null)
            .GroupBy(t => t.AssigneeId)
            .Select(g => new EmployeePerformanceDto
            {
                UserId = g.Key,
                UserName = g.First().Assignee?.FullName ?? "Unknown",
                TotalAssigned = g.Count(),
                TotalCompleted = g.Count(t => t.Status == QssTaskStatus.Completed),
                CompletionRate = g.Count() > 0 ? Math.Round((double)g.Count(t => t.Status == QssTaskStatus.Completed) / g.Count() * 100, 1) : 0
            })
            .OrderByDescending(p => p.CompletionRate)
            .Take(5)
            .ToList();

        var recentTasks = tasks
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Name = t.Name,
                Status = t.Status.ToString(),
                DueDate = t.DueDate,
                AssigneeName = t.Assignee?.FullName,
                ProcessName = t.Process?.Name,
                CreatedAt = t.CreatedAt
            }).ToList();

        return new DashboardDto
        {
            QssScore = qssScore,
            TotalTasks = total,
            CompletedTasks = completed,
            OverdueTasks = overdue,
            OpenTasks = open,
            InProgressTasks = inProgress,
            TotalUsers = users,
            LowStockMaterials = lowStock,
            ExpiringMedications = expMeds,
            Alerts = alerts,
            RecentTasks = recentTasks,
            TopPerformers = perf
        };
    }
}
