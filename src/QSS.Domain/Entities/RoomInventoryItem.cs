using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class RoomInventoryItem : BaseEntity
{
    public int RoomId { get; set; }
    public int MaterialId { get; set; }
    public int Quantity { get; set; }

    // Navigation
    public Room? Room { get; set; }
    public Material? Material { get; set; }
}
