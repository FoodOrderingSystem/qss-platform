using QSS.Domain.Common;
using QSS.Domain.Enums;

namespace QSS.Domain.Entities;

public class QssTask : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QssTaskStatus Status { get; set; } = QssTaskStatus.Open;
    public TaskFrequency Frequency { get; set; } = TaskFrequency.Once;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign keys
    public int ProcessId { get; set; }
    public string AssigneeId { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public int? RoomId { get; set; }
    public int? DeviceId { get; set; }
    public int? DependsOnTaskId { get; set; }

    // Navigation
    public QssProcess? Process { get; set; }
    public ApplicationUser? Assignee { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
    public Room? Room { get; set; }
    public Device? Device { get; set; }
    public QssTask? DependsOnTask { get; set; }
    public ICollection<QssTask> DependentTasks { get; set; } = new List<QssTask>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
