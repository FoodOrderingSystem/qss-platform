using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class LearningMaterial : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // pdf, video, doc
    public long FileSizeBytes { get; set; }
    public string? Category { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;

    // Navigation
    public ApplicationUser? UploadedBy { get; set; }
    public ICollection<LearningProgress> LearningProgress { get; set; } = new List<LearningProgress>();
}
