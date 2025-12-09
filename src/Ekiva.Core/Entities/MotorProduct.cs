using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class MotorProduct : BaseEntity
{
    public string ProductCode { get; set; } = string.Empty; // Ex: "MOTOR_TPO", "MOTOR_COMP"
    public string ProductName { get; set; } = string.Empty; // "Third Party Only", "Comprehensive"
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Relations
    public ICollection<MotorProductCoverage> ProductCoverages { get; set; } = new List<MotorProductCoverage>();
}
