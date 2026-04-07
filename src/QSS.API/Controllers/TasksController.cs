using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QSS.Application.DTOs;
using QSS.Domain.Entities;
using QSS.Domain.Enums;
using QSS.Infrastructure.Data;
using QSS.API.Hubs;
using System.Security.Claims;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<NotificationHub> _hub;

    public TasksController(ApplicationDbContext db, IHubContext<NotificationHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? assigneeId)
    {
        var query = _db.Tasks
            .Include(t => t.Process)
            .Include(t => t.Assignee)
            .Include(t => t.Room)
            .Include(t => t.Device)
            .Include(t => t.DependsOnTask)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<QssTaskStatus>(status, true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(assigneeId))
            query = query.Where(t => t.AssigneeId == assigneeId);

        var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(tasks.Select(t => MapToDto(t)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var t = await _db.Tasks
            .Include(t => t.Process)
            .Include(t => t.Assignee)
            .Include(t => t.Room)
            .Include(t => t.Device)
            .Include(t => t.DependsOnTask)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (t == null) return NotFound();

        var dto = MapToDto(t);
        dto.Comments = t.Comments.OrderBy(c => c.CreatedAt).Select(c => new TaskCommentDto
        {
            Id = c.Id,
            Content = c.Content,
            AuthorId = c.AuthorId,
            AuthorName = c.Author?.FullName ?? "",
            TaskId = c.TaskId,
            MentionedUserId = c.MentionedUserId,
            CreatedAt = c.CreatedAt
        }).ToList();

        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin,Dentist")]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var task = new QssTask
        {
            Name = dto.Name,
            Description = dto.Description,
            Status = dto.Status,
            Frequency = dto.Frequency,
            DueDate = dto.DueDate,
            ProcessId = dto.ProcessId,
            AssigneeId = dto.AssigneeId,
            RoomId = dto.RoomId,
            DeviceId = dto.DeviceId,
            DependsOnTaskId = dto.DependsOnTaskId,
            AttachmentUrl = dto.AttachmentUrl,
            AttachmentName = dto.AttachmentName,
            CreatedByUserId = userId
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        // Notify assignee via SignalR
        var notification = new AppNotification
        {
            UserId = dto.AssigneeId,
            Title = "New Task Assigned",
            Message = $"You have been assigned: {dto.Name}",
            Type = NotificationType.TaskAssigned,
            ResourceUrl = $"/tasks/{task.Id}"
        };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
        await _hub.Clients.User(dto.AssigneeId).SendAsync("ReceiveNotification", notification.Title, notification.Message);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, new { task.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superadmin,Admin,Dentist")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateTaskDto dto)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        task.Name = dto.Name;
        task.Description = dto.Description;
        task.Frequency = dto.Frequency;
        task.DueDate = dto.DueDate;
        task.ProcessId = dto.ProcessId;
        task.AssigneeId = dto.AssigneeId;
        task.RoomId = dto.RoomId;
        task.DeviceId = dto.DeviceId;
        task.DependsOnTaskId = dto.DependsOnTaskId;
        task.AttachmentUrl = dto.AttachmentUrl;
        task.AttachmentName = dto.AttachmentName;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Superadmin,Admin,Dentist,DentalAssistant")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusDto dto)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        task.Status = dto.Status;
        if (dto.Status == QssTaskStatus.Completed)
            task.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Trigger: when a task is completed, activate any tasks that depend on it
        if (dto.Status == QssTaskStatus.Completed)
            await ActivateDependentTasks(id);

        // Trigger: log device history when a device-linked task is completed
        if (dto.Status == QssTaskStatus.Completed && task.DeviceId.HasValue)
            await LogDeviceTaskCompletion(task);

        return NoContent();
    }

    [HttpPost("{id}/comments")]
    [Authorize(Roles = "Superadmin,Admin,Dentist,DentalAssistant")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!string.IsNullOrEmpty(dto.MentionedUserId))
        {
            var mentionedUser = await _db.Users.FindAsync(dto.MentionedUserId);
            if (mentionedUser == null)
                return BadRequest(new { message = "Mentioned user does not exist" });
        }

        var comment = new TaskComment
        {
            TaskId = id,
            Content = dto.Content,
            AuthorId = userId,
            MentionedUserId = dto.MentionedUserId
        };
        _db.TaskComments.Add(comment);
        await _db.SaveChangesAsync();

        // Notify mentioned user
        if (!string.IsNullOrEmpty(dto.MentionedUserId))
        {
            var notification = new AppNotification
            {
                UserId = dto.MentionedUserId,
                Title = "You were mentioned",
                Message = "You were mentioned in a task comment",
                Type = NotificationType.NewComment,
                ResourceUrl = $"/tasks/{id}"
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
            await _hub.Clients.User(dto.MentionedUserId).SendAsync("ReceiveNotification", notification.Title, notification.Message);
        }

        return Ok(new { comment.Id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        task.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task ActivateDependentTasks(int completedTaskId)
    {
        var dependentTasks = await _db.Tasks
            .Where(t => t.DependsOnTaskId == completedTaskId && t.Status == QssTaskStatus.Open)
            .ToListAsync();

        foreach (var dep in dependentTasks)
        {
            // Mark as InProgress to signal it is now unblocked and ready
            dep.Status = QssTaskStatus.InProgress;
        }

        if (dependentTasks.Any())
            await _db.SaveChangesAsync();
    }

    private async Task LogDeviceTaskCompletion(QssTask task)
    {
        var historyEntry = new DeviceHistory
        {
            DeviceId = task.DeviceId!.Value,
            EventType = "TaskCompleted",
            Title = $"Task completed: {task.Name}",
            Notes = task.Description,
            EventDate = task.CompletedAt ?? DateTime.UtcNow,
            PerformedByUserId = null,
            LinkedTaskId = task.Id
        };
        _db.DeviceHistories.Add(historyEntry);
        await _db.SaveChangesAsync();
    }

    private static TaskDto MapToDto(QssTask t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        Status = t.Status.ToString(),
        Frequency = t.Frequency.ToString(),
        DueDate = t.DueDate,
        CompletedAt = t.CompletedAt,
        AttachmentUrl = t.AttachmentUrl,
        AttachmentName = t.AttachmentName,
        ProcessId = t.ProcessId,
        ProcessName = t.Process?.Name,
        AssigneeId = t.AssigneeId,
        AssigneeName = t.Assignee?.FullName,
        RoomId = t.RoomId,
        RoomName = t.Room?.Name,
        DeviceId = t.DeviceId,
        DeviceName = t.Device?.Name,
        DependsOnTaskId = t.DependsOnTaskId,
        DependsOnTaskName = t.DependsOnTask?.Name,
        IsBlocked = t.DependsOnTaskId.HasValue && t.DependsOnTask != null && t.DependsOnTask.Status != QssTaskStatus.Completed,
        CreatedAt = t.CreatedAt
    };
}
