using QSS.Domain.Enums;

namespace QSS.Application.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public int ProcessId { get; set; }
    public string? ProcessName { get; set; }
    public string AssigneeId { get; set; } = string.Empty;
    public string? AssigneeName { get; set; }
    public int? RoomId { get; set; }
    public string? RoomName { get; set; }
    public int? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public int? DependsOnTaskId { get; set; }
    public string? DependsOnTaskName { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TaskCommentDto> Comments { get; set; } = new();
}

public class CreateTaskDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QssTaskStatus Status { get; set; } = QssTaskStatus.Open;
    public TaskFrequency Frequency { get; set; } = TaskFrequency.Once;
    public DateTime? DueDate { get; set; }
    public int ProcessId { get; set; }
    public string AssigneeId { get; set; } = string.Empty;
    public int? RoomId { get; set; }
    public int? DeviceId { get; set; }
    public int? DependsOnTaskId { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
}

public class UpdateTaskStatusDto
{
    public QssTaskStatus Status { get; set; }
}

