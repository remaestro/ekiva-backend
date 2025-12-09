using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class CommissionRate : BaseEntity
{
    public DistributorType DistributorType { get; set; }
    public ProductType ProductType { get; set; }
    public decimal Rate { get; set; } // e.g., 0.15 for 15%
    public string Description { get; set; } = string.Empty;
}
