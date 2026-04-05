using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class LearningProgress : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int LearningMaterialId { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercent { get; set; } = 0;

    // Navigation
    public ApplicationUser? User { get; set; }
    public LearningMaterial? LearningMaterial { get; set; }
}
