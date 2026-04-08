using QSS.Domain.Common;
using QSS.Domain.Enums;

namespace QSS.Domain.Entities;

public class QssProcess : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // Legacy enum field kept for backward compatibility with existing data
    public ProcessCategory Category { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedByUserId { get; set; } = string.Empty;

    // Managed category (replaces the hardcoded enum for new processes)
    public int? ProcessCategoryId { get; set; }
    public ProcessCategoryItem? ProcessCategory { get; set; }

    // Navigation properties
    public ApplicationUser? CreatedBy { get; set; }
    public ICollection<QssTask> Tasks { get; set; } = new List<QssTask>();
}
