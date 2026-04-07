namespace QSS.Application.DTOs;

public class RoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int DeviceCount { get; set; }
    public int OpenTaskCount { get; set; }
    public int OverdueTaskCount { get; set; }
}

public class RoomDetailDto : RoomDto
{
    public List<RoomDeviceSummaryDto> Devices { get; set; } = new();
    public List<RoomTaskSummaryDto> Tasks { get; set; } = new();
}

public class RoomDeviceSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? NextMaintenanceDue { get; set; }
    public bool MaintenanceOverdue { get; set; }
}

public class RoomTaskSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? AssigneeName { get; set; }
    public bool IsOverdue { get; set; }
}

public class CreateRoomDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
}

public class DeviceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDue { get; set; }
    public string? QrCode { get; set; }
    public int? RoomId { get; set; }
    public string? RoomName { get; set; }
    public bool MaintenanceOverdue { get; set; }
}

public class DeviceDetailDto : DeviceDto
{
    public List<DeviceTaskSummaryDto> Tasks { get; set; } = new();
    public List<DeviceHistoryDto> History { get; set; } = new();
}

public class DeviceTaskSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? AssigneeName { get; set; }
    public bool IsOverdue { get; set; }
}

public class DeviceHistoryDto
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime EventDate { get; set; }
    public string? PerformedByName { get; set; }
    public string? LinkedTaskName { get; set; }
    public int? LinkedTaskId { get; set; }
}

public class CreateDeviceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? NextMaintenanceDue { get; set; }
    public int? RoomId { get; set; }
}

public class AddDeviceHistoryDto
{
    public string EventType { get; set; } = "Note";
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? EventDate { get; set; }
}
