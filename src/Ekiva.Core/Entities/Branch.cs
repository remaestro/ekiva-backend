using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class Branch : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Agence Plateau"
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    public Guid SubsidiaryId { get; set; }
    public virtual Subsidiary Subsidiary { get; set; } = null!;
}
