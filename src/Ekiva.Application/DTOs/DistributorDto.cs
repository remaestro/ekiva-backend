using Ekiva.Core.Entities;

namespace Ekiva.Application.DTOs;

/// <summary>
/// DTO pour afficher un distributeur
/// </summary>
public class DistributorDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "InternalAgent", "GeneralAgent", "Broker", "Bancassurance"
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO pour créer un distributeur
/// </summary>
public class CreateDistributorDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "InternalAgent"; // Default
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO pour mettre à jour un distributeur
/// </summary>
public class UpdateDistributorDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO pour la recherche de distributeurs
/// </summary>
public class DistributorSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
