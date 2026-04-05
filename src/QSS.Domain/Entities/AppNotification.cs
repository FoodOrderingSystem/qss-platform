using QSS.Domain.Common;
using QSS.Domain.Enums;

namespace QSS.Domain.Entities;

public class AppNotification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public string? ResourceUrl { get; set; }

    // Navigation
    public ApplicationUser? User { get; set; }
}
