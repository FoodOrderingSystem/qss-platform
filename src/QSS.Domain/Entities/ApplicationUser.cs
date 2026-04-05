using Microsoft.AspNetCore.Identity;

namespace QSS.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<QssTask> AssignedTasks { get; set; } = new List<QssTask>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
    public ICollection<AppNotification> Notifications { get; set; } = new List<AppNotification>();
    public ICollection<LearningProgress> LearningProgress { get; set; } = new List<LearningProgress>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
