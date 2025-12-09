using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class ProfessionalCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty; // e.g., "MEDECIN"
    public string Name { get; set; } = string.Empty; // e.g., "Médecin / Pharmacien"
    public decimal DiscountPercentage { get; set; } // e.g., 0.20 for 20%
    public bool IsActive { get; set; } = true;
}
