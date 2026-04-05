namespace QSS.Application.DTOs;

public class RoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int DeviceCount { get; set; }
    public int OpenTaskCount { get; set; }
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
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? NextMaintenanceDue { get; set; }
    public string? QrCode { get; set; }
    public int? RoomId { get; set; }
    public string? RoomName { get; set; }
    public bool MaintenanceOverdue { get; set; }
}

public class CreateDeviceDto
{
    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? NextMaintenanceDue { get; set; }
    public int? RoomId { get; set; }
}
