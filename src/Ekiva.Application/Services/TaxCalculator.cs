using Ekiva.Application.DTOs.Tax;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.Application.Services;

/// <summary>
/// Service de calcul des taxes selon les règles CIMA
/// </summary>
public interface ITaxCalculator
{
    Task<TaxCalculationResponse> CalculateTaxesAsync(TaxCalculationRequest request);
    Task<List<ProductTaxRate>> GetTaxRatesForProductAsync(ProductType productType);
}

public class TaxCalculator : ITaxCalculator
{
    private readonly IRepository<ProductTaxRate> _taxRateRepo;

    public TaxCalculator(IRepository<ProductTaxRate> taxRateRepo)
    {
        _taxRateRepo = taxRateRepo;
    }

    /// <summary>
    /// Calcule les taxes et frais selon les règles CIMA
    /// </summary>
    public async Task<TaxCalculationResponse> CalculateTaxesAsync(TaxCalculationRequest request)
    {
        // 1. Récupérer les taux de taxes pour le produit
        var taxRates = await GetTaxRatesForProductAsync(request.ProductType);

        var taxItems = new List<TaxItem>();
        decimal totalTaxAmount = 0;

        // 2. Si des taux sont configurés en base, les utiliser
        if (taxRates.Any())
        {
            foreach (var taxRate in taxRates)
            {
                var amount = request.NetPremium * taxRate.Rate;
                totalTaxAmount += amount;

                taxItems.Add(new TaxItem
                {
                    TaxName = taxRate.TaxName,
                    Rate = taxRate.Rate,
                    RatePercentage = taxRate.Rate * 100,
                    Amount = amount,
                    IsFee = taxRate.IsFee
                });
            }
        }
        else
        {
            // 3. Sinon, utiliser les taux par défaut selon les règles métier CIMA
            var defaultRates = GetDefaultTaxRates(request.ProductType);
            foreach (var (taxName, rate, isFee) in defaultRates)
            {
                var amount = request.NetPremium * rate;
                totalTaxAmount += amount;

                taxItems.Add(new TaxItem
                {
                    TaxName = taxName,
                    Rate = rate,
                    RatePercentage = rate * 100,
                    Amount = amount,
                    IsFee = isFee
                });
            }
        }

        return new TaxCalculationResponse
        {
            NetPremium = request.NetPremium,
            ProductType = request.ProductType,
            ProductTypeLabel = GetProductTypeLabel(request.ProductType),
            Taxes = taxItems,
            TotalTaxAmount = totalTaxAmount,
            GrossPremium = request.NetPremium + totalTaxAmount
        };
    }

    /// <summary>
    /// Récupère les taux de taxes configurés pour un produit
    /// </summary>
    public async Task<List<ProductTaxRate>> GetTaxRatesForProductAsync(ProductType productType)
    {
        var query = _taxRateRepo.GetQueryable();
        return await query
            .Where(ptr => ptr.ProductType == productType)
            .ToListAsync();
    }

    /// <summary>
    /// Retourne les taux de taxes par défaut selon les règles CIMA
    /// </summary>
    private List<(string TaxName, decimal Rate, bool IsFee)> GetDefaultTaxRates(ProductType productType)
    {
        return productType switch
        {
            ProductType.Motor => new List<(string, decimal, bool)>
            {
                ("Taxes", 0.145m, false),           // 14.5%
                ("Frais de contrôle", 0.0125m, true) // 1.25%
            },
            ProductType.Fire => new List<(string, decimal, bool)>
            {
                ("Taxes", 0.25m, false),            // 25%
                ("Frais de contrôle", 0.0125m, true) // 1.25%
            },
            ProductType.Liability => new List<(string, decimal, bool)>
            {
                ("Taxes", 0.145m, false),           // 14.5%
                ("Frais de contrôle", 0.0125m, true) // 1.25%
            },
            ProductType.Transport => new List<(string, decimal, bool)>
            {
                ("Taxes", 0.145m, false),           // 14.5% (peut varier entre 7% et 14.5%)
                ("Frais de contrôle", 0.0125m, true) // 1.25%
            },
            _ => new List<(string, decimal, bool)>
            {
                ("Taxes", 0.145m, false),
                ("Frais de contrôle", 0.0125m, true)
            }
        };
    }

    private string GetProductTypeLabel(ProductType type)
    {
        return type switch
        {
            ProductType.Motor => "Automobile",
            ProductType.Fire => "Incendie",
            ProductType.Liability => "RC Générale",
            ProductType.Transport => "Transport",
            ProductType.Health => "Santé",
            _ => type.ToString()
        };
    }
}
