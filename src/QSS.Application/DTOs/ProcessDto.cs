using QSS.Domain.Enums;

namespace QSS.Application.DTOs;

public class ProcessDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProcessDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProcessCategory Category { get; set; }
}
