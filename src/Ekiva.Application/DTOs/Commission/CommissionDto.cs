using Ekiva.Core.Entities;

namespace Ekiva.Application.DTOs.Commission;

/// <summary>
/// DTO pour le calcul de commission
/// </summary>
public class CommissionCalculationRequest
{
    public decimal NetPremium { get; set; }
    public decimal LifePremium { get; set; } = 0; // Prime vie à exclure si applicable
    public DistributorType DistributorType { get; set; }
    public ProductType ProductType { get; set; }
    public Guid? DistributorId { get; set; } // Optionnel pour traçabilité
}

/// <summary>
/// DTO pour le résultat du calcul de commission
/// </summary>
public class CommissionCalculationResponse
{
    public decimal NetPremium { get; set; }
    public decimal LifePremium { get; set; }
    public decimal CommissionableAmount { get; set; } // NetPremium - LifePremium
    public decimal CommissionRate { get; set; } // Taux en décimal (ex: 0.15 pour 15%)
    public decimal CommissionRatePercentage { get; set; } // Taux en pourcentage (ex: 15)
    public decimal CommissionAmount { get; set; }
    public DistributorType DistributorType { get; set; }
    public string DistributorTypeLabel { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public bool HasMandateTax { get; set; } // Applicable aux agents mandataires
    public decimal MandateTaxRate { get; set; } = 0.075m; // 7.5%
    public decimal MandateTaxAmount { get; set; } = 0;
    public decimal NetCommission { get; set; } // Commission après taxe de mandat
}

/// <summary>
/// DTO pour afficher un taux de commission
/// </summary>
public class CommissionRateDto
{
    public Guid Id { get; set; }
    public string DistributorType { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal RatePercentage => Rate * 100;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO pour créer/modifier un taux de commission
/// </summary>
public class CreateCommissionRateDto
{
    public string DistributorType { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour mise à jour d'un taux de commission
/// </summary>
public class UpdateCommissionRateDto : CreateCommissionRateDto { }
