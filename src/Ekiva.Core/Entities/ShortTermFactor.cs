using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class ShortTermFactor : BaseEntity
{
    public int Months { get; set; } // Durée en mois (1, 3, 6, 9, 12)
    public decimal Coefficient { get; set; } // Coefficient multiplicateur (ex: 0.25, 0.40, etc.)
    public bool IsActive { get; set; } = true;
}
