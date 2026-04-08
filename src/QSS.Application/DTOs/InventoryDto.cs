using QSS.Domain.Enums;

namespace QSS.Application.DTOs;

public class MaterialDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierLink { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsExpiringSoon { get; set; }
    public string? Category { get; set; }
    public int? RoomId { get; set; }
    public string? RoomName { get; set; }
}

public class CreateMaterialDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierLink { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; } = 5;
    public MaterialUnit Unit { get; set; } = MaterialUnit.Pieces;
    public DateTime? ExpirationDate { get; set; }
    public string? Category { get; set; }
    public int? RoomId { get; set; }
}

public class MedicationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ActiveIngredient { get; set; }
    public string? Dosage { get; set; }
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsExpiringSoon { get; set; }
}

public class CreateMedicationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ActiveIngredient { get; set; }
    public string? Dosage { get; set; }
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; } = 2;
    public DateTime? ExpirationDate { get; set; }
    public string? StorageInstructions { get; set; }
    public string? BatchNumber { get; set; }
}
