namespace Ekiva.Application.DTOs.Motor;

public class CreateMotorQuoteDto
{
    // Client & Distributeur
    public Guid ClientId { get; set; }
    public Guid? DistributorId { get; set; }
    
    // Produit
    public Guid MotorProductId { get; set; }
    
    // Période
    public DateTime PolicyStartDate { get; set; }
    public DateTime PolicyEndDate { get; set; }
    public int DurationMonths { get; set; }
    
    // Véhicule
    public Guid VehicleCategoryId { get; set; }
    public Guid VehicleMakeId { get; set; }
    public Guid VehicleModelId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public int YearOfManufacture { get; set; }
    public int Horsepower { get; set; }
    public string FuelType { get; set; } = string.Empty; // "Essence", "Diesel"
    public decimal VehicleValue { get; set; }
    
    // Garanties sélectionnées
    public List<Guid> SelectedCoverageIds { get; set; } = new();
    
    // Remises
    public decimal ProfessionalDiscountPercent { get; set; }
    public decimal CommercialDiscountPercent { get; set; }
    
    // Devise
    public Guid CurrencyId { get; set; }
    
    // Notes
    public string? Notes { get; set; }
}

public class MotorQuoteResponseDto
{
    public Guid Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public DateTime QuoteDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public string ClientName { get; set; } = string.Empty;
    public string? DistributorName { get; set; }
    public string ProductName { get; set; } = string.Empty;
    
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public decimal VehicleValue { get; set; }
    
    public decimal NetPremium { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PolicyCostAmount { get; set; }
    public decimal TotalPremium { get; set; }
    
    public string CurrencyCode { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
}
