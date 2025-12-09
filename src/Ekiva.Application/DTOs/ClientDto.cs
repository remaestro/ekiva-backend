namespace Ekiva.Application.DTOs;

public class ClientDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Individual" or "Company"
    
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Individual
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Profession { get; set; }
    
    // Company
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? TradeRegister { get; set; }
    public string? ContactPersonName { get; set; }

    public string FullName { get; set; } = string.Empty;
}

public class CreateClientDto
{
    public string Type { get; set; } = "Individual";
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Individual
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Profession { get; set; }
    
    // Company
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? TradeRegister { get; set; }
    public string? ContactPersonName { get; set; }
}

/// <summary>
/// DTO pour mettre à jour un client
/// </summary>
public class UpdateClientDto
{
    public string Type { get; set; } = "Individual";
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    
    // Individual
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Profession { get; set; }
    
    // Company
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? TradeRegister { get; set; }
    public string? ContactPersonName { get; set; }
}

/// <summary>
/// DTO pour la recherche de clients
/// </summary>
public class ClientSearchDto
{
    public string? SearchTerm { get; set; } // Recherche dans nom, email, téléphone, référence
    public string? Type { get; set; } // "Individual" ou "Company"
    public string? City { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
