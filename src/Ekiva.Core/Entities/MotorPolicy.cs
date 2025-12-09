using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public enum PolicyStatus
{
    Draft,
    Active,
    Suspended,
    Cancelled,
    Expired
}

public class MotorPolicy : BaseEntity
{
    public string PolicyNumber { get; set; } = string.Empty; // Généré automatiquement (ex: POL-2024-12-0001)
    public DateTime PolicyDate { get; set; } = DateTime.UtcNow;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    
    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;
    
    // Référence au devis source
    public Guid? MotorQuoteId { get; set; }
    public MotorQuote? MotorQuote { get; set; }
    public string? QuoteNumber { get; set; } // Copie pour référence
    
    // Client
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    
    // Distributeur (optionnel)
    public Guid? DistributorId { get; set; }
    public Distributor? Distributor { get; set; }
    
    // Produit
    public Guid MotorProductId { get; set; }
    public MotorProduct MotorProduct { get; set; } = null!;
    
    // Période de couverture
    public DateTime PolicyStartDate { get; set; }
    public DateTime PolicyEndDate { get; set; }
    public int DurationMonths { get; set; }
    
    // Véhicule
    public Guid VehicleCategoryId { get; set; }
    public VehicleCategory VehicleCategory { get; set; } = null!;
    
    public Guid VehicleMakeId { get; set; }
    public VehicleMake VehicleMake { get; set; } = null!;
    
    public Guid VehicleModelId { get; set; }
    public VehicleModel VehicleModel { get; set; } = null!;
    
    public string RegistrationNumber { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public int YearOfManufacture { get; set; }
    public int Horsepower { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal VehicleValue { get; set; }
    
    // Calculs financiers (copiés du devis)
    public decimal BasePremium { get; set; }
    public decimal SectionsPremium { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ProfessionalDiscountPercent { get; set; }
    public decimal CommercialDiscountPercent { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetPremiumBeforeShortTerm { get; set; }
    public decimal ShortTermCoefficient { get; set; } = 1.0m;
    public decimal NetPremium { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PolicyCostAmount { get; set; }
    public decimal TotalPremium { get; set; }
    
    // Commission
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    
    // Devise
    public Guid CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
    
    // Paiement
    public bool IsPaid { get; set; } = false;
    public DateTime? PaymentDate { get; set; }
    public string? PaymentReference { get; set; }
    
    // Notes
    public string? Notes { get; set; }
    
    // Relations
    public ICollection<MotorPolicyCoverage> Coverages { get; set; } = new List<MotorPolicyCoverage>();
    public ICollection<MotorPolicyEndorsement> Endorsements { get; set; } = new List<MotorPolicyEndorsement>(); // Avenants
}

// Garanties de la police
public class MotorPolicyCoverage : BaseEntity
{
    public Guid MotorPolicyId { get; set; }
    public MotorPolicy MotorPolicy { get; set; } = null!;
    
    public Guid MotorCoverageId { get; set; }
    public MotorCoverage MotorCoverage { get; set; } = null!;
    
    public bool IsActive { get; set; } = true;
    public decimal PremiumAmount { get; set; }
}

// Avenants (modifications de police)
public class MotorPolicyEndorsement : BaseEntity
{
    public Guid MotorPolicyId { get; set; }
    public MotorPolicy MotorPolicy { get; set; } = null!;
    
    public string EndorsementNumber { get; set; } = string.Empty; // AVE-2024-12-0001
    public DateTime EndorsementDate { get; set; } = DateTime.UtcNow;
    public string EndorsementType { get; set; } = string.Empty; // "AddCoverage", "RemoveCoverage", "ChangeVehicleValue", "Suspension", "Cancellation"
    public string Description { get; set; } = string.Empty;
    
    // Impact financier
    public decimal PremiumAdjustment { get; set; } // Peut être positif ou négatif
    public decimal NewTotalPremium { get; set; }
    
    public DateTime EffectiveDate { get; set; }
    public string? Reason { get; set; }
}
