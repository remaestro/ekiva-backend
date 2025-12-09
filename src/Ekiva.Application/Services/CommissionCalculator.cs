using Ekiva.Application.DTOs.Commission;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.Application.Services;

/// <summary>
/// Service de calcul des commissions
/// </summary>
public interface ICommissionCalculator
{
    Task<CommissionCalculationResponse> CalculateCommissionAsync(CommissionCalculationRequest request);
    Task<decimal> GetCommissionRateAsync(DistributorType distributorType, ProductType productType);
}

public class CommissionCalculator : ICommissionCalculator
{
    private readonly IRepository<CommissionRate> _commissionRateRepo;

    public CommissionCalculator(IRepository<CommissionRate> commissionRateRepo)
    {
        _commissionRateRepo = commissionRateRepo;
    }

    /// <summary>
    /// Calcule la commission selon les règles métier EKIVA
    /// Formule: Commission = (NetPremium - LifePremium) × TauxCommission
    /// </summary>
    public async Task<CommissionCalculationResponse> CalculateCommissionAsync(CommissionCalculationRequest request)
    {
        // 1. Récupérer le taux de commission
        var commissionRate = await GetCommissionRateAsync(request.DistributorType, request.ProductType);

        // 2. Calculer le montant commissionnable (exclure la prime vie)
        var commissionableAmount = request.NetPremium - request.LifePremium;

        // 3. Calculer le montant de la commission
        var commissionAmount = commissionableAmount * commissionRate;

        // 4. Déterminer si la taxe de mandat s'applique (Agents Mandataires uniquement)
        var hasMandateTax = request.DistributorType == DistributorType.InternalAgent || 
                            request.DistributorType == DistributorType.GeneralAgent;

        // 5. Calculer la taxe de mandat si applicable (7.5%)
        var mandateTaxAmount = hasMandateTax ? commissionAmount * 0.075m : 0;

        // 6. Commission nette après taxe de mandat
        var netCommission = commissionAmount - mandateTaxAmount;

        return new CommissionCalculationResponse
        {
            NetPremium = request.NetPremium,
            LifePremium = request.LifePremium,
            CommissionableAmount = commissionableAmount,
            CommissionRate = commissionRate,
            CommissionRatePercentage = commissionRate * 100,
            CommissionAmount = commissionAmount,
            DistributorType = request.DistributorType,
            DistributorTypeLabel = GetDistributorTypeLabel(request.DistributorType),
            ProductType = request.ProductType,
            HasMandateTax = hasMandateTax,
            MandateTaxRate = 0.075m,
            MandateTaxAmount = mandateTaxAmount,
            NetCommission = netCommission
        };
    }

    /// <summary>
    /// Récupère le taux de commission pour un distributeur et un produit
    /// </summary>
    public async Task<decimal> GetCommissionRateAsync(DistributorType distributorType, ProductType productType)
    {
        var query = _commissionRateRepo.GetQueryable();
        var rate = await query
            .Where(cr => cr.DistributorType == distributorType && cr.ProductType == productType)
            .FirstOrDefaultAsync();

        if (rate != null)
        {
            return rate.Rate;
        }

        // Taux par défaut selon les règles métier EKIVA
        return distributorType switch
        {
            DistributorType.InternalAgent => 0.10m,  // 10%
            DistributorType.Broker => 0.125m,        // 12.5%
            DistributorType.GeneralAgent => 0.15m,   // 15%
            DistributorType.Bancassurance => 0.08m,  // 8%
            _ => 0.10m
        };
    }

    private string GetDistributorTypeLabel(DistributorType type)
    {
        return type switch
        {
            DistributorType.InternalAgent => "Agent Interne",
            DistributorType.GeneralAgent => "Agent Général",
            DistributorType.Broker => "Courtier",
            DistributorType.Bancassurance => "Bancassurance",
            _ => type.ToString()
        };
    }
}
