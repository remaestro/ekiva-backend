using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class RatingFactor : BaseEntity
{
    public int HorsepowerMin { get; set; } // Puissance fiscale min (CV)
    public int HorsepowerMax { get; set; } // Puissance fiscale max (CV)
    public string FuelType { get; set; } = string.Empty; // "Essence", "Diesel"
    public decimal RatePercentage { get; set; } // Taux en pourcentage (ex: 2.50 pour 2.50%)
    public bool IsActive { get; set; } = true;
}
