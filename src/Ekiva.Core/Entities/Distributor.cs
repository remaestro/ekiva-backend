using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public enum DistributorType
{
    InternalAgent, // Agent salarié
    GeneralAgent,  // Agent Général
    Broker,        // Courtier
    Bancassurance  // Banque
}

public class Distributor : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DistributorType Type { get; set; }
    
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}
