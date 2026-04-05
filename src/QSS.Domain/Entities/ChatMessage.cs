using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string? RecipientId { get; set; }   // null = team/broadcast
    public string? TeamName { get; set; }
    public bool IsRead { get; set; } = false;

    // Navigation
    public ApplicationUser? Sender { get; set; }
    public ApplicationUser? Recipient { get; set; }
}
