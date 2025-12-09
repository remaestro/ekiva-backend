using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class MotorProductCoverage : BaseEntity
{
    public Guid MotorProductId { get; set; }
    public MotorProduct MotorProduct { get; set; } = null!;
    
    public Guid MotorCoverageId { get; set; }
    public MotorCoverage MotorCoverage { get; set; } = null!;
    
    public bool IsIncludedByDefault { get; set; } // Incluse par défaut dans le produit
    public decimal? CustomPremium { get; set; } // Prime personnalisée pour ce produit (override)
}
