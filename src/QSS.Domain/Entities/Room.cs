using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class Room : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<QssTask> Tasks { get; set; } = new List<QssTask>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<RoomInventoryItem> InventoryItems { get; set; } = new List<RoomInventoryItem>();
}
