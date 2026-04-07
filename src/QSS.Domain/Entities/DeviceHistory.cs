using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class DeviceHistory : BaseEntity
{
    public int DeviceId { get; set; }
    public string EventType { get; set; } = string.Empty; // Maintenance, TaskCompleted, Note, Update
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public string? PerformedByUserId { get; set; }
    public int? LinkedTaskId { get; set; }

    // Navigation
    public Device? Device { get; set; }
    public ApplicationUser? PerformedBy { get; set; }
    public QssTask? LinkedTask { get; set; }
}
