using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class ProcessCategoryItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    // Navigation
    public ICollection<QssProcess> Processes { get; set; } = new List<QssProcess>();
}
