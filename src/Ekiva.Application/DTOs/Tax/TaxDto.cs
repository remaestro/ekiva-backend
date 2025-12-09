using Ekiva.Core.Entities;

namespace Ekiva.Application.DTOs.Tax;

/// <summary>
/// DTO pour le calcul des taxes
/// </summary>
public class TaxCalculationRequest
{
    public decimal NetPremium { get; set; }
    public ProductType ProductType { get; set; }
}

/// <summary>
/// DTO pour le résultat du calcul de taxes
/// </summary>
public class TaxCalculationResponse
{
    public decimal NetPremium { get; set; }
    public ProductType ProductType { get; set; }
    public string ProductTypeLabel { get; set; } = string.Empty;
    public List<TaxItem> Taxes { get; set; } = new();
    public decimal TotalTaxAmount { get; set; }
    public decimal GrossPremium { get; set; } // NetPremium + TotalTaxAmount
}

/// <summary>
/// Détail d'une taxe ou frais
/// </summary>
public class TaxItem
{
    public string TaxName { get; set; } = string.Empty;
    public decimal Rate { get; set; } // Taux en décimal (ex: 0.145)
    public decimal RatePercentage { get; set; } // Taux en pourcentage (ex: 14.5)
    public decimal Amount { get; set; }
    public bool IsFee { get; set; }
}

/// <summary>
/// DTO pour afficher un taux de taxe
/// </summary>
public class ProductTaxRateDto
{
    public Guid Id { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal RatePercentage => Rate * 100;
    public bool IsFee { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO pour créer/modifier un taux de taxe
/// </summary>
public class CreateProductTaxRateDto
{
    public string ProductType { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsFee { get; set; }
}

/// <summary>
/// DTO pour mise à jour d'un taux de taxe
/// </summary>
public class UpdateProductTaxRateDto : CreateProductTaxRateDto { }
