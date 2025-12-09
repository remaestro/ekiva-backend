using AutoMapper;
using Ekiva.Application.DTOs.Tax;
using Ekiva.Application.Services;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ekiva.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaxesController : ControllerBase
{
    private readonly ITaxCalculator _taxCalculator;
    private readonly IRepository<ProductTaxRate> _taxRateRepo;
    private readonly IMapper _mapper;

    public TaxesController(
        ITaxCalculator taxCalculator,
        IRepository<ProductTaxRate> taxRateRepo,
        IMapper mapper)
    {
        _taxCalculator = taxCalculator;
        _taxRateRepo = taxRateRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// Calculer les taxes et frais pour un produit
    /// </summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<TaxCalculationResponse>> Calculate([FromBody] TaxCalculationRequest request)
    {
        var result = await _taxCalculator.CalculateTaxesAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Obtenir les taux de taxes pour un produit
    /// </summary>
    [HttpGet("rates/{productType}")]
    public async Task<ActionResult<List<ProductTaxRate>>> GetRatesForProduct(string productType)
    {
        if (!Enum.TryParse<ProductType>(productType, out var prodType))
            return BadRequest("Type de produit invalide");

        var rates = await _taxCalculator.GetTaxRatesForProductAsync(prodType);
        return Ok(rates);
    }

    /// <summary>
    /// Obtenir tous les taux de taxes configurés
    /// </summary>
    [HttpGet("rates")]
    public async Task<ActionResult<List<ProductTaxRateDto>>> GetAllRates()
    {
        var rates = await _taxRateRepo.ListAllAsync();
        return Ok(_mapper.Map<List<ProductTaxRateDto>>(rates));
    }

    /// <summary>
    /// Obtenir un taux de taxe par ID
    /// </summary>
    [HttpGet("rates/detail/{id}")]
    public async Task<ActionResult<ProductTaxRateDto>> GetRateById(Guid id)
    {
        var rate = await _taxRateRepo.GetByIdAsync(id);
        if (rate == null) return NotFound();
        return Ok(_mapper.Map<ProductTaxRateDto>(rate));
    }

    /// <summary>
    /// Créer un nouveau taux de taxe
    /// </summary>
    [HttpPost("rates")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProductTaxRateDto>> CreateRate([FromBody] CreateProductTaxRateDto createDto)
    {
        if (!Enum.TryParse<ProductType>(createDto.ProductType, out var prodType))
            return BadRequest("Type de produit invalide");

        var rate = new ProductTaxRate
        {
            ProductType = prodType,
            TaxName = createDto.TaxName,
            Rate = createDto.Rate,
            IsFee = createDto.IsFee
        };

        await _taxRateRepo.AddAsync(rate);
        return CreatedAtAction(nameof(GetRateById), new { id = rate.Id }, _mapper.Map<ProductTaxRateDto>(rate));
    }

    /// <summary>
    /// Mettre à jour un taux de taxe
    /// </summary>
    [HttpPut("rates/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProductTaxRateDto>> UpdateRate(Guid id, [FromBody] UpdateProductTaxRateDto updateDto)
    {
        var rate = await _taxRateRepo.GetByIdAsync(id);
        if (rate == null) return NotFound();

        if (!Enum.TryParse<ProductType>(updateDto.ProductType, out var prodType))
            return BadRequest("Type de produit invalide");

        rate.ProductType = prodType;
        rate.TaxName = updateDto.TaxName;
        rate.Rate = updateDto.Rate;
        rate.IsFee = updateDto.IsFee;

        await _taxRateRepo.UpdateAsync(rate);
        return Ok(_mapper.Map<ProductTaxRateDto>(rate));
    }

    /// <summary>
    /// Supprimer un taux de taxe
    /// </summary>
    [HttpDelete("rates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteRate(Guid id)
    {
        var rate = await _taxRateRepo.GetByIdAsync(id);
        if (rate == null) return NotFound();

        await _taxRateRepo.DeleteAsync(rate);
        return NoContent();
    }

    /// <summary>
    /// Obtenir les statistiques des taxes
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var rates = await _taxRateRepo.ListAllAsync();
        
        return Ok(new
        {
            TotalRates = rates.Count,
            ByProductType = rates.GroupBy(r => r.ProductType)
                .Select(g => new { ProductType = g.Key.ToString(), Count = g.Count() }),
            TaxesCount = rates.Count(r => !r.IsFee),
            FeesCount = rates.Count(r => r.IsFee),
            AverageRate = rates.Any() ? rates.Average(r => r.Rate) : 0,
            AverageRatePercentage = rates.Any() ? rates.Average(r => r.Rate) * 100 : 0
        });
    }
}
