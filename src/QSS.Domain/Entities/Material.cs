using QSS.Domain.Common;
using QSS.Domain.Enums;

namespace QSS.Domain.Entities;

public class Material : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierLink { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; } = 5;
    public MaterialUnit Unit { get; set; } = MaterialUnit.Pieces;
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; }

    // Navigation
    public ICollection<RoomInventoryItem> RoomInventoryItems { get; set; } = new List<RoomInventoryItem>();
}
