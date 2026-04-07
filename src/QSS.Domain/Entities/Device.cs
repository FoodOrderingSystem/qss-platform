using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class Device : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDue { get; set; }
    public string? QrCode { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign keys
    public int? RoomId { get; set; }

    // Navigation
    public Room? Room { get; set; }
    public ICollection<QssTask> Tasks { get; set; } = new List<QssTask>();
    public ICollection<DeviceHistory> History { get; set; } = new List<DeviceHistory>();
}
