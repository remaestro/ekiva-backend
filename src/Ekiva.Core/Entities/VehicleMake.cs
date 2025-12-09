using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class VehicleMake : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; // e.g., "Toyota"
    public virtual ICollection<VehicleModel> Models { get; set; } = new List<VehicleModel>();
}
