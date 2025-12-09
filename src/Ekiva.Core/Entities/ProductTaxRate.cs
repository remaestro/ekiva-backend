using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class ProductTaxRate : BaseEntity
{
    public ProductType ProductType { get; set; }
    public string TaxName { get; set; } = string.Empty; // e.g., "Taxes", "Frais de contrôle"
    public decimal Rate { get; set; } // e.g., 0.145 for 14.5%
    public bool IsFee { get; set; } // True if it's a fixed fee or specific fee, False if it's a tax
}
