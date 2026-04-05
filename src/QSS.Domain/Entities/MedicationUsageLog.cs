using QSS.Domain.Common;

namespace QSS.Domain.Entities;

public class MedicationUsageLog : BaseEntity
{
    public int MedicationId { get; set; }
    public int QuantityUsed { get; set; }
    public string UsedByUserId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int? TaskId { get; set; }

    // Navigation
    public Medication? Medication { get; set; }
    public ApplicationUser? UsedBy { get; set; }
}
