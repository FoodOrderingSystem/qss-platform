namespace QSS.Application.DTOs;

public class LearningMaterialDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Category { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public string? UploadedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UserProgressPercent { get; set; }
    public bool? UserCompleted { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
