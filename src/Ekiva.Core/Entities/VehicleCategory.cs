using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class VehicleCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty; // e.g., "TOURISME"
    public string Name { get; set; } = string.Empty; // e.g., "Promenade et Affaires"
    public string Description { get; set; } = string.Empty;
}
