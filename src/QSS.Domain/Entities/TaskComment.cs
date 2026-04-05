using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class TaskComment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string? MentionedUserId { get; set; }

    // Navigation
    public QssTask? Task { get; set; }
    public ApplicationUser? Author { get; set; }
    public ApplicationUser? MentionedUser { get; set; }
}
