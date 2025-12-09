using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class PolicyCost : BaseEntity
{
    public decimal NetPremiumMin { get; set; } // Prime nette minimum
    public decimal? NetPremiumMax { get; set; } // Prime nette maximum (null = illimité)
    public decimal CostAmount { get; set; } // Montant des frais
    public string ProductCode { get; set; } = "MOTOR"; // Produit concerné
    public bool IsActive { get; set; } = true;
}
