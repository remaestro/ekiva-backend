using Ekiva.Application.DTOs.Motor;
using Ekiva.Application.Services;
using Ekiva.Core.Entities;
using Ekiva.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.Infrastructure.Services;

public class MotorPolicyService : IMotorPolicyService
{
    private readonly EkivaDbContext _context;

    public MotorPolicyService(EkivaDbContext context)
    {
        _context = context;
    }

    public async Task<MotorPolicyDto> ConvertQuoteToPolicyAsync(Guid quoteId)
    {
        // 1. Récupérer le devis avec toutes les relations
        var quote = await _context.MotorQuotes
            .Include(q => q.Client)
            .Include(q => q.Distributor)
            .Include(q => q.MotorProduct)
            .Include(q => q.VehicleCategory)
            .Include(q => q.VehicleMake)
            .Include(q => q.VehicleModel)
            .Include(q => q.Currency)
            .Include(q => q.SelectedCoverages)
                .ThenInclude(sc => sc.MotorCoverage)
            .FirstOrDefaultAsync(q => q.Id == quoteId);

        if (quote == null)
            throw new Exception($"Devis {quoteId} non trouvé");

        // 2. Vérifier que le devis est accepté
        if (quote.Status != QuoteStatus.Accepted)
            throw new Exception("Le devis doit être accepté avant la conversion en police");

        // 3. Vérifier qu'une police n'existe pas déjà pour ce devis
        var existingPolicy = await _context.MotorPolicies
            .FirstOrDefaultAsync(p => p.MotorQuoteId == quoteId);

        if (existingPolicy != null)
            throw new Exception("Une police existe déjà pour ce devis");

        // 4. Générer le numéro de police
        var policyNumber = await GeneratePolicyNumberAsync();

        // 5. Créer la police à partir du devis
        var policy = new MotorPolicy
        {
            Id = Guid.NewGuid(),
            PolicyNumber = policyNumber,
            PolicyDate = DateTime.UtcNow,
            IssueDate = DateTime.UtcNow,
            Status = PolicyStatus.Draft, // Draft jusqu'au paiement
            
            // Référence au devis
            MotorQuoteId = quote.Id,
            QuoteNumber = quote.QuoteNumber,
            
            // Client
            ClientId = quote.ClientId,
            
            // Distributeur
            DistributorId = quote.DistributorId,
            
            // Produit
            MotorProductId = quote.MotorProductId,
            
            // Période
            PolicyStartDate = quote.PolicyStartDate,
            PolicyEndDate = quote.PolicyEndDate,
            DurationMonths = quote.DurationMonths,
            
            // Véhicule
            VehicleCategoryId = quote.VehicleCategoryId,
            VehicleMakeId = quote.VehicleMakeId,
            VehicleModelId = quote.VehicleModelId,
            RegistrationNumber = quote.RegistrationNumber,
            ChassisNumber = quote.ChassisNumber,
            YearOfManufacture = quote.YearOfManufacture,
            Horsepower = quote.Horsepower,
            FuelType = quote.FuelType,
            VehicleValue = quote.VehicleValue,
            
            // Calculs financiers
            BasePremium = quote.BasePremium,
            SectionsPremium = quote.SectionsPremium,
            Subtotal = quote.Subtotal,
            ProfessionalDiscountPercent = quote.ProfessionalDiscountPercent,
            CommercialDiscountPercent = quote.CommercialDiscountPercent,
            TotalDiscount = quote.TotalDiscount,
            NetPremiumBeforeShortTerm = quote.NetPremiumBeforeShortTerm,
            ShortTermCoefficient = quote.ShortTermCoefficient,
            NetPremium = quote.NetPremium,
            TaxAmount = quote.TaxAmount,
            PolicyCostAmount = quote.PolicyCostAmount,
            TotalPremium = quote.TotalPremium,
            
            // Commission
            CommissionRate = quote.CommissionRate,
            CommissionAmount = quote.CommissionAmount,
            
            // Devise
            CurrencyId = quote.CurrencyId,
            
            // Notes
            Notes = quote.Notes
        };

        // 6. Copier les garanties du devis vers la police
        foreach (var quoteCoverage in quote.SelectedCoverages)
        {
            policy.Coverages.Add(new MotorPolicyCoverage
            {
                Id = Guid.NewGuid(),
                MotorCoverageId = quoteCoverage.MotorCoverageId,
                IsActive = true,
                PremiumAmount = quoteCoverage.PremiumAmount
            });
        }

        // 7. Sauvegarder la police
        _context.MotorPolicies.Add(policy);
        await _context.SaveChangesAsync();

        // 8. Retourner le DTO
        return await GetPolicyByIdAsync(policy.Id);
    }

    public async Task<MotorPolicyDto> GetPolicyByIdAsync(Guid policyId)
    {
        var policy = await _context.MotorPolicies
            .Include(p => p.Client)
            .Include(p => p.Distributor)
            .Include(p => p.MotorProduct)
            .Include(p => p.VehicleCategory)
            .Include(p => p.VehicleMake)
            .Include(p => p.VehicleModel)
            .Include(p => p.Currency)
            .Include(p => p.Coverages)
                .ThenInclude(c => c.MotorCoverage)
            .Include(p => p.Endorsements)
            .FirstOrDefaultAsync(p => p.Id == policyId);

        if (policy == null)
            throw new Exception($"Police {policyId} non trouvée");

        return MapPolicyToDto(policy);
    }

    public async Task<List<MotorPolicyDto>> GetPoliciesByClientIdAsync(Guid clientId)
    {
        var policies = await _context.MotorPolicies
            .Include(p => p.Client)
            .Include(p => p.Distributor)
            .Include(p => p.MotorProduct)
            .Include(p => p.VehicleCategory)
            .Include(p => p.VehicleMake)
            .Include(p => p.VehicleModel)
            .Include(p => p.Currency)
            .Include(p => p.Coverages)
                .ThenInclude(c => c.MotorCoverage)
            .Include(p => p.Endorsements)
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return policies.Select(MapPolicyToDto).ToList();
    }

    public async Task<MotorPolicyDto> ActivatePolicyAsync(Guid policyId, string paymentReference)
    {
        var policy = await _context.MotorPolicies.FindAsync(policyId);
        
        if (policy == null)
            throw new Exception($"Police {policyId} non trouvée");

        if (policy.Status != PolicyStatus.Draft)
            throw new Exception("Seule une police en brouillon peut être activée");

        policy.Status = PolicyStatus.Active;
        policy.IsPaid = true;
        policy.PaymentDate = DateTime.UtcNow;
        policy.PaymentReference = paymentReference;
        policy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetPolicyByIdAsync(policyId);
    }

    public async Task<MotorPolicyDto> SuspendPolicyAsync(Guid policyId, string reason)
    {
        var policy = await _context.MotorPolicies.FindAsync(policyId);
        
        if (policy == null)
            throw new Exception($"Police {policyId} non trouvée");

        if (policy.Status != PolicyStatus.Active)
            throw new Exception("Seule une police active peut être suspendue");

        policy.Status = PolicyStatus.Suspended;
        policy.UpdatedAt = DateTime.UtcNow;

        // Créer un avenant de suspension
        var endorsement = new MotorPolicyEndorsement
        {
            Id = Guid.NewGuid(),
            MotorPolicyId = policyId,
            EndorsementNumber = await GenerateEndorsementNumberAsync(),
            EndorsementDate = DateTime.UtcNow,
            EndorsementType = "Suspension",
            Description = "Suspension de la police",
            PremiumAdjustment = 0,
            NewTotalPremium = policy.TotalPremium,
            EffectiveDate = DateTime.UtcNow,
            Reason = reason
        };

        _context.MotorPolicyEndorsements.Add(endorsement);
        await _context.SaveChangesAsync();

        return await GetPolicyByIdAsync(policyId);
    }

    public async Task<MotorPolicyDto> CancelPolicyAsync(Guid policyId, string reason)
    {
        var policy = await _context.MotorPolicies.FindAsync(policyId);
        
        if (policy == null)
            throw new Exception($"Police {policyId} non trouvée");

        policy.Status = PolicyStatus.Cancelled;
        policy.UpdatedAt = DateTime.UtcNow;

        // Créer un avenant d'annulation
        var endorsement = new MotorPolicyEndorsement
        {
            Id = Guid.NewGuid(),
            MotorPolicyId = policyId,
            EndorsementNumber = await GenerateEndorsementNumberAsync(),
            EndorsementDate = DateTime.UtcNow,
            EndorsementType = "Cancellation",
            Description = "Annulation de la police",
            PremiumAdjustment = 0,
            NewTotalPremium = 0,
            EffectiveDate = DateTime.UtcNow,
            Reason = reason
        };

        _context.MotorPolicyEndorsements.Add(endorsement);
        await _context.SaveChangesAsync();

        return await GetPolicyByIdAsync(policyId);
    }

    public async Task<MotorPolicyEndorsementDto> CreateEndorsementAsync(Guid policyId, CreateEndorsementDto request)
    {
        var policy = await _context.MotorPolicies
            .Include(p => p.Coverages)
            .FirstOrDefaultAsync(p => p.Id == policyId);

        if (policy == null)
            throw new Exception($"Police {policyId} non trouvée");

        decimal premiumAdjustment = 0;

        // Gérer les différents types d'avenants
        switch (request.EndorsementType)
        {
            case "AddCoverage":
                if (request.CoverageIdsToAdd != null && request.CoverageIdsToAdd.Any())
                {
                    // TODO: Calculer l'ajustement de prime et ajouter les garanties
                    premiumAdjustment = 5000; // Exemple simplifié
                }
                break;

            case "RemoveCoverage":
                if (request.CoverageIdsToRemove != null && request.CoverageIdsToRemove.Any())
                {
                    // TODO: Calculer l'ajustement de prime et retirer les garanties
                    premiumAdjustment = -5000; // Exemple simplifié
                }
                break;

            case "ChangeVehicleValue":
                if (request.NewVehicleValue.HasValue)
                {
                    // TODO: Recalculer la prime basée sur la nouvelle valeur
                    premiumAdjustment = (request.NewVehicleValue.Value - policy.VehicleValue) * 0.025m;
                    policy.VehicleValue = request.NewVehicleValue.Value;
                }
                break;
        }

        var newTotalPremium = policy.TotalPremium + premiumAdjustment;

        var endorsement = new MotorPolicyEndorsement
        {
            Id = Guid.NewGuid(),
            MotorPolicyId = policyId,
            EndorsementNumber = await GenerateEndorsementNumberAsync(),
            EndorsementDate = DateTime.UtcNow,
            EndorsementType = request.EndorsementType,
            Description = request.Description,
            PremiumAdjustment = premiumAdjustment,
            NewTotalPremium = newTotalPremium,
            EffectiveDate = request.EffectiveDate,
            Reason = request.Reason
        };

        policy.TotalPremium = newTotalPremium;
        policy.UpdatedAt = DateTime.UtcNow;

        _context.MotorPolicyEndorsements.Add(endorsement);
        await _context.SaveChangesAsync();

        return new MotorPolicyEndorsementDto
        {
            Id = endorsement.Id,
            EndorsementNumber = endorsement.EndorsementNumber,
            EndorsementDate = endorsement.EndorsementDate,
            EndorsementType = endorsement.EndorsementType,
            Description = endorsement.Description,
            PremiumAdjustment = endorsement.PremiumAdjustment,
            NewTotalPremium = endorsement.NewTotalPremium,
            EffectiveDate = endorsement.EffectiveDate,
            Reason = endorsement.Reason
        };
    }

    public async Task<List<MotorPolicyEndorsementDto>> GetPolicyEndorsementsAsync(Guid policyId)
    {
        var endorsements = await _context.MotorPolicyEndorsements
            .Where(e => e.MotorPolicyId == policyId)
            .OrderByDescending(e => e.EndorsementDate)
            .ToListAsync();

        return endorsements.Select(e => new MotorPolicyEndorsementDto
        {
            Id = e.Id,
            EndorsementNumber = e.EndorsementNumber,
            EndorsementDate = e.EndorsementDate,
            EndorsementType = e.EndorsementType,
            Description = e.Description,
            PremiumAdjustment = e.PremiumAdjustment,
            NewTotalPremium = e.NewTotalPremium,
            EffectiveDate = e.EffectiveDate,
            Reason = e.Reason
        }).ToList();
    }

    // Méthodes privées

    private async Task<string> GeneratePolicyNumberAsync()
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        // Compter les polices du mois en cours
        var count = await _context.MotorPolicies
            .Where(p => p.PolicyDate.Year == year && p.PolicyDate.Month == month)
            .CountAsync();

        var sequenceNumber = (count + 1).ToString("D4");

        return $"POL-{year}-{month:D2}-{sequenceNumber}";
    }

    private async Task<string> GenerateEndorsementNumberAsync()
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        // Compter les avenants du mois en cours
        var count = await _context.MotorPolicyEndorsements
            .Where(e => e.EndorsementDate.Year == year && e.EndorsementDate.Month == month)
            .CountAsync();

        var sequenceNumber = (count + 1).ToString("D4");

        return $"AVE-{year}-{month:D2}-{sequenceNumber}";
    }

    private MotorPolicyDto MapPolicyToDto(MotorPolicy policy)
    {
        return new MotorPolicyDto
        {
            Id = policy.Id,
            PolicyNumber = policy.PolicyNumber,
            PolicyDate = policy.PolicyDate,
            IssueDate = policy.IssueDate,
            Status = policy.Status.ToString(),
            QuoteNumber = policy.QuoteNumber,
            
            ClientId = policy.ClientId,
            ClientName = policy.Client.Type == ClientType.Individual 
                ? $"{policy.Client.FirstName} {policy.Client.LastName}"
                : policy.Client.CompanyName ?? string.Empty,
            
            DistributorName = policy.Distributor?.Name,
            ProductName = policy.MotorProduct.ProductName,
            
            PolicyStartDate = policy.PolicyStartDate,
            PolicyEndDate = policy.PolicyEndDate,
            DurationMonths = policy.DurationMonths,
            
            RegistrationNumber = policy.RegistrationNumber,
            VehicleMake = policy.VehicleMake.Name,
            VehicleModel = policy.VehicleModel.Name,
            VehicleCategory = policy.VehicleCategory.Name,
            YearOfManufacture = policy.YearOfManufacture,
            VehicleValue = policy.VehicleValue,
            
            NetPremium = policy.NetPremium,
            TaxAmount = policy.TaxAmount,
            PolicyCostAmount = policy.PolicyCostAmount,
            TotalPremium = policy.TotalPremium,
            CommissionAmount = policy.CommissionAmount,
            
            CurrencyCode = policy.Currency.Code,
            
            IsPaid = policy.IsPaid,
            PaymentDate = policy.PaymentDate,
            PaymentReference = policy.PaymentReference,
            
            Coverages = policy.Coverages.Select(c => new PolicyCoverageDto
            {
                Id = c.Id,
                CoverageCode = c.MotorCoverage.CoverageCode,
                CoverageName = c.MotorCoverage.CoverageName,
                SectionLetter = c.MotorCoverage.SectionLetter,
                PremiumAmount = c.PremiumAmount,
                IsActive = c.IsActive
            }).ToList(),
            
            Endorsements = policy.Endorsements.Select(e => new MotorPolicyEndorsementDto
            {
                Id = e.Id,
                EndorsementNumber = e.EndorsementNumber,
                EndorsementDate = e.EndorsementDate,
                EndorsementType = e.EndorsementType,
                Description = e.Description,
                PremiumAdjustment = e.PremiumAdjustment,
                NewTotalPremium = e.NewTotalPremium,
                EffectiveDate = e.EffectiveDate,
                Reason = e.Reason
            }).ToList(),
            
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }
}
