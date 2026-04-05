using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class Medication : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ActiveIngredient { get; set; }
    public string? Dosage { get; set; }
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; } = 2;
    public DateTime? ExpirationDate { get; set; }
    public string? StorageInstructions { get; set; }
    public bool IsActive { get; set; } = true;
    public string? BatchNumber { get; set; }

    // Navigation
    public ICollection<MedicationUsageLog> UsageLogs { get; set; } = new List<MedicationUsageLog>();
}
