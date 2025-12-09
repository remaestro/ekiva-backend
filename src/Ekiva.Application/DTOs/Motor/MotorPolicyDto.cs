namespace Ekiva.Application.DTOs.Motor;

public class MotorPolicyDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime PolicyDate { get; set; }
    public DateTime IssueDate { get; set; }
    public string Status { get; set; } = string.Empty; // "Draft", "Active", "Suspended", "Cancelled", "Expired"
    
    // Référence au devis
    public string? QuoteNumber { get; set; }
    
    // Client
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    
    // Distributeur
    public string? DistributorName { get; set; }
    
    // Produit
    public string ProductName { get; set; } = string.Empty;
    
    // Période
    public DateTime PolicyStartDate { get; set; }
    public DateTime PolicyEndDate { get; set; }
    public int DurationMonths { get; set; }
    
    // Véhicule
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string VehicleCategory { get; set; } = string.Empty;
    public int YearOfManufacture { get; set; }
    public decimal VehicleValue { get; set; }
    
    // Financier
    public decimal NetPremium { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PolicyCostAmount { get; set; }
    public decimal TotalPremium { get; set; }
    public decimal CommissionAmount { get; set; }
    
    // Devise
    public string CurrencyCode { get; set; } = string.Empty;
    
    // Paiement
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentReference { get; set; }
    
    // Garanties
    public List<PolicyCoverageDto> Coverages { get; set; } = new();
    
    // Avenants
    public List<MotorPolicyEndorsementDto> Endorsements { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PolicyCoverageDto
{
    public Guid Id { get; set; }
    public string CoverageCode { get; set; } = string.Empty;
    public string CoverageName { get; set; } = string.Empty;
    public string SectionLetter { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
    public bool IsActive { get; set; }
}

public class MotorPolicyEndorsementDto
{
    public Guid Id { get; set; }
    public string EndorsementNumber { get; set; } = string.Empty;
    public DateTime EndorsementDate { get; set; }
    public string EndorsementType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PremiumAdjustment { get; set; }
    public decimal NewTotalPremium { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Reason { get; set; }
}

public class CreateEndorsementDto
{
    public string EndorsementType { get; set; } = string.Empty; // "AddCoverage", "RemoveCoverage", "ChangeVehicleValue", "Suspension", "Cancellation"
    public string Description { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public string? Reason { get; set; }
    
    // Pour AddCoverage
    public List<Guid>? CoverageIdsToAdd { get; set; }
    
    // Pour RemoveCoverage
    public List<Guid>? CoverageIdsToRemove { get; set; }
    
    // Pour ChangeVehicleValue
    public decimal? NewVehicleValue { get; set; }
}
