using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty; // e.g., "XOF"
    public string Name { get; set; } = string.Empty; // e.g., "Franc CFA"
    public string Symbol { get; set; } = string.Empty; // e.g., "FCFA"
    public bool IsActive { get; set; } = true;
}
