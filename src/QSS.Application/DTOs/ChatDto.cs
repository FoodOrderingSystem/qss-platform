namespace QSS.Application.DTOs;

public class ChatMessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? RecipientId { get; set; }
    public string? TeamName { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageDto
{
    public string Content { get; set; } = string.Empty;
    public string? RecipientId { get; set; }
    public string? TeamName { get; set; }
}

public class TaskCommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public string? MentionedUserId { get; set; }
    public string? MentionedUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddCommentDto
{
    public string Content { get; set; } = string.Empty;
    public string? MentionedUserId { get; set; }
}
