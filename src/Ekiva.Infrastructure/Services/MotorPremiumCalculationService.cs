using Ekiva.Application.Services;
using Ekiva.Core.Entities;
using Ekiva.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.Infrastructure.Services;

public class MotorPremiumCalculationService : IMotorPremiumCalculationService
{
    private readonly EkivaDbContext _context;
    private const decimal TAX_RATE = 0.145m; // 14.5% pour automobile

    public MotorPremiumCalculationService(EkivaDbContext context)
    {
        _context = context;
    }

    public async Task<MotorPremiumCalculationResult> CalculatePremiumAsync(MotorPremiumCalculationRequest request)
    {
        var result = new MotorPremiumCalculationResult();

        // 1. Calcul de la Prime de Base
        var ratingFactor = await GetRatingFactorAsync(request.Horsepower, request.FuelType);
        result.BasePremium = request.VehicleValue * (ratingFactor / 100);

        // 2. Calcul de la Prime des Sections (Garanties)
        var coverages = await _context.MotorCoverages
            .Where(c => request.SelectedCoverageIds.Contains(c.Id))
            .ToListAsync();

        foreach (var coverage in coverages)
        {
            result.CoverageDetails.Add(new CoverageCalculation
            {
                CoverageId = coverage.Id,
                CoverageName = coverage.CoverageName,
                PremiumAmount = coverage.FixedPremium
            });
            result.SectionsPremium += coverage.FixedPremium;
        }

        // 3. Sous-Total
        result.Subtotal = result.BasePremium + result.SectionsPremium;

        // 4. Remises (Discounts)
        var totalDiscountPercent = request.ProfessionalDiscountPercent + request.CommercialDiscountPercent;
        result.TotalDiscount = result.Subtotal * (totalDiscountPercent / 100);
        result.NetPremiumBeforeShortTerm = result.Subtotal - result.TotalDiscount;

        // 5. Ajustement Court Terme
        result.ShortTermCoefficient = await GetShortTermCoefficientAsync(request.DurationMonths);
        result.NetPremium = result.NetPremiumBeforeShortTerm * result.ShortTermCoefficient;

        // 6. Taxes (14.5%)
        result.TaxAmount = result.NetPremium * TAX_RATE;

        // 7. Frais Accessoires (Policy Cost)
        result.PolicyCostAmount = await GetPolicyCostAsync(result.NetPremium);

        // 8. Prime Totale
        result.TotalPremium = result.NetPremium + result.TaxAmount + result.PolicyCostAmount;

        // 9. Commission
        if (request.DistributorId.HasValue)
        {
            result.CommissionRate = await GetCommissionRateAsync(request.DistributorId.Value);
            result.CommissionAmount = result.NetPremium * (result.CommissionRate / 100);
        }

        return result;
    }

    private async Task<decimal> GetRatingFactorAsync(int horsepower, string fuelType)
    {
        var ratingFactor = await _context.RatingFactors
            .Where(r => r.HorsepowerMin <= horsepower 
                     && r.HorsepowerMax >= horsepower 
                     && r.FuelType == fuelType
                     && r.IsActive)
            .FirstOrDefaultAsync();

        return ratingFactor?.RatePercentage ?? 2.50m; // Default: 2.50%
    }

    private async Task<decimal> GetShortTermCoefficientAsync(int months)
    {
        var shortTermFactor = await _context.ShortTermFactors
            .Where(s => s.Months == months && s.IsActive)
            .FirstOrDefaultAsync();

        return shortTermFactor?.Coefficient ?? 1.0m; // Default: 12 mois = 1.0
    }

    private async Task<decimal> GetPolicyCostAsync(decimal netPremium)
    {
        var policyCost = await _context.PolicyCosts
            .Where(p => p.NetPremiumMin <= netPremium 
                     && (p.NetPremiumMax == null || p.NetPremiumMax >= netPremium)
                     && p.ProductCode == "MOTOR"
                     && p.IsActive)
            .FirstOrDefaultAsync();

        return policyCost?.CostAmount ?? 1000m; // Default: 1,000 FCFA
    }

    private async Task<decimal> GetCommissionRateAsync(Guid distributorId)
    {
        var distributor = await _context.Distributors.FindAsync(distributorId);
        if (distributor == null) return 0m;

        var commissionRate = await _context.CommissionRates
            .Where(c => c.DistributorType == distributor.Type 
                     && c.ProductType == ProductType.Motor)
            .FirstOrDefaultAsync();

        return commissionRate?.Rate ?? 0.10m; // Default: 10% (0.10)
    }
}
