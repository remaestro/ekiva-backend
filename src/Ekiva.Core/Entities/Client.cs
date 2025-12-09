using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public enum ClientType
{
    Individual,
    Company
}

public class Client : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty; // Code unique client
    public ClientType Type { get; set; }
    
    // Champs communs
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Champs Individu
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; } // M/F
    public string? Profession { get; set; }
    
    // Champs Entreprise
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; } // N° CC
    public string? TradeRegister { get; set; } // RCCM
    public string? ContactPersonName { get; set; }

    public string FullName => Type == ClientType.Individual ? $"{FirstName} {LastName}" : CompanyName ?? string.Empty;
}
