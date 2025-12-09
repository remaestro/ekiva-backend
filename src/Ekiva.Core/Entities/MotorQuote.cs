using Ekiva.Core.Common;

namespace Ekiva.Core.Entities;

public enum QuoteStatus
{
    Draft,
    Generated,
    Accepted,
    Rejected,
    Expired
}

public class MotorQuote : BaseEntity
{
    public string QuoteNumber { get; set; } = string.Empty; // Généré automatiquement
    public DateTime QuoteDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; } // Validité du devis (généralement 30 jours)
    
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    
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
    public int Horsepower { get; set; } // Puissance fiscale (CV)
    public string FuelType { get; set; } = string.Empty; // "Essence", "Diesel"
    public decimal VehicleValue { get; set; } // Valeur à neuf ou vénale
    
    // Calculs
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
    
    // Notes
    public string? Notes { get; set; }
    
    // Relations
    public ICollection<MotorQuoteCoverage> SelectedCoverages { get; set; } = new List<MotorQuoteCoverage>();
}
