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

public class ProcessDetailDto : ProcessDto
{
    public int CompletedTaskCount { get; set; }
    public int OverdueTaskCount { get; set; }
    public List<ProcessTaskSummaryDto> Tasks { get; set; } = new();
}

public class ProcessTaskSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? AssigneeName { get; set; }
    public bool IsOverdue { get; set; }
}

public class CreateProcessDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProcessCategory Category { get; set; }
}
