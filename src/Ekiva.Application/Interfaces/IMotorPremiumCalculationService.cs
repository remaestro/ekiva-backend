using Ekiva.Core.Entities;

namespace Ekiva.Application.Services;

public interface IMotorPremiumCalculationService
{
    Task<MotorPremiumCalculationResult> CalculatePremiumAsync(MotorPremiumCalculationRequest request);
}

public class MotorPremiumCalculationRequest
{
    public decimal VehicleValue { get; set; }
    public int Horsepower { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public List<Guid> SelectedCoverageIds { get; set; } = new();
    public decimal ProfessionalDiscountPercent { get; set; }
    public decimal CommercialDiscountPercent { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid? DistributorId { get; set; }
}

public class MotorPremiumCalculationResult
{
    public decimal BasePremium { get; set; }
    public decimal SectionsPremium { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetPremiumBeforeShortTerm { get; set; }
    public decimal ShortTermCoefficient { get; set; }
    public decimal NetPremium { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PolicyCostAmount { get; set; }
    public decimal TotalPremium { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public List<CoverageCalculation> CoverageDetails { get; set; } = new();
}

public class CoverageCalculation
{
    public Guid CoverageId { get; set; }
    public string CoverageName { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
}
