using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class Subsidiary : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Ekiva Côte d'Ivoire"
    public string CountryCode { get; set; } = string.Empty; // e.g., "CI"
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
