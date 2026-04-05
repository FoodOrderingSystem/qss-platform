using QSS.Domain.Common;
using QSS.Domain.Enums;

namespace QSS.Domain.Entities;

public class QssProcess : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProcessCategory Category { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedByUserId { get; set; } = string.Empty;

    // Navigation properties
    public ApplicationUser? CreatedBy { get; set; }
    public ICollection<QssTask> Tasks { get; set; } = new List<QssTask>();
}
