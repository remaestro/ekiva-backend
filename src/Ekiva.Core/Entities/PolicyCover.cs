using System;

namespace Ekiva.Core.Entities
{
    public class PolicyCover
    {
        public Guid Id { get; set; }
        
        public Guid PolicyRiskId { get; set; }
        public PolicyRisk PolicyRisk { get; set; } = null!;

        public string CoverCode { get; set; } = string.Empty; // RC, THEFT, FIRE, GLASS...
        public string CoverName { get; set; } = string.Empty;

        public decimal SumInsured { get; set; } // Capital assuré pour cette garantie
        public decimal PremiumRate { get; set; } // Taux appliqué (si applicable)
        public decimal PremiumAmount { get; set; } // Montant de la prime pour cette garantie
    }
}
