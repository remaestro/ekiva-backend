using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class VehicleModel : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; // e.g., "Corolla"
    
    public Guid MakeId { get; set; }
    public virtual VehicleMake Make { get; set; } = null!;
}
