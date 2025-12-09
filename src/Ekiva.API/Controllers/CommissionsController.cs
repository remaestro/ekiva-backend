using AutoMapper;
using Ekiva.Application.DTOs.Commission;
using Ekiva.Application.Services;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CommissionsController : ControllerBase
{
    private readonly ICommissionCalculator _commissionCalculator;
    private readonly IRepository<CommissionRate> _commissionRateRepo;
    private readonly IMapper _mapper;

    public CommissionsController(
        ICommissionCalculator commissionCalculator,
        IRepository<CommissionRate> commissionRateRepo,
        IMapper mapper)
    {
        _commissionCalculator = commissionCalculator;
        _commissionRateRepo = commissionRateRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// Calculer la commission pour un devis/police
    /// </summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<CommissionCalculationResponse>> Calculate([FromBody] CommissionCalculationRequest request)
    {
        var result = await _commissionCalculator.CalculateCommissionAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Obtenir le taux de commission pour un distributeur et un produit
    /// </summary>
    [HttpGet("rate")]
    public async Task<ActionResult<decimal>> GetRate([FromQuery] string distributorType, [FromQuery] string productType)
    {
        if (!Enum.TryParse<DistributorType>(distributorType, out var distType))
            return BadRequest("Type de distributeur invalide");

        if (!Enum.TryParse<ProductType>(productType, out var prodType))
            return BadRequest("Type de produit invalide");

        var rate = await _commissionCalculator.GetCommissionRateAsync(distType, prodType);
        return Ok(new { Rate = rate, RatePercentage = rate * 100 });
    }

    /// <summary>
    /// Obtenir tous les taux de commission configurés
    /// </summary>
    [HttpGet("rates")]
    public async Task<ActionResult<List<CommissionRateDto>>> GetAllRates()
    {
        var rates = await _commissionRateRepo.ListAllAsync();
        return Ok(_mapper.Map<List<CommissionRateDto>>(rates));
    }

    /// <summary>
    /// Obtenir un taux de commission par ID
    /// </summary>
    [HttpGet("rates/{id}")]
    public async Task<ActionResult<CommissionRateDto>> GetRateById(Guid id)
    {
        var rate = await _commissionRateRepo.GetByIdAsync(id);
        if (rate == null) return NotFound();
        return Ok(_mapper.Map<CommissionRateDto>(rate));
    }

    /// <summary>
    /// Créer un nouveau taux de commission
    /// </summary>
    [HttpPost("rates")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CommissionRateDto>> CreateRate([FromBody] CreateCommissionRateDto createDto)
    {
        if (!Enum.TryParse<DistributorType>(createDto.DistributorType, out var distType))
            return BadRequest("Type de distributeur invalide");

        if (!Enum.TryParse<ProductType>(createDto.ProductType, out var prodType))
            return BadRequest("Type de produit invalide");

        var rate = new CommissionRate
        {
            DistributorType = distType,
            ProductType = prodType,
            Rate = createDto.Rate,
            Description = createDto.Description
        };

        await _commissionRateRepo.AddAsync(rate);
        return CreatedAtAction(nameof(GetRateById), new { id = rate.Id }, _mapper.Map<CommissionRateDto>(rate));
    }

    /// <summary>
    /// Mettre à jour un taux de commission
    /// </summary>
    [HttpPut("rates/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CommissionRateDto>> UpdateRate(Guid id, [FromBody] UpdateCommissionRateDto updateDto)
    {
        var rate = await _commissionRateRepo.GetByIdAsync(id);
        if (rate == null) return NotFound();

        if (!Enum.TryParse<DistributorType>(updateDto.DistributorType, out var distType))
            return BadRequest("Type de distributeur invalide");

        if (!Enum.TryParse<ProductType>(updateDto.ProductType, out var prodType))
            return BadRequest("Type de produit invalide");

        rate.DistributorType = distType;
        rate.ProductType = prodType;
        rate.Rate = updateDto.Rate;
        rate.Description = updateDto.Description;

        await _commissionRateRepo.UpdateAsync(rate);
        return Ok(_mapper.Map<CommissionRateDto>(rate));
    }

    /// <summary>
    /// Supprimer un taux de commission
    /// </summary>
    [HttpDelete("rates/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteRate(Guid id)
    {
        var rate = await _commissionRateRepo.GetByIdAsync(id);
        if (rate == null) return NotFound();

        await _commissionRateRepo.DeleteAsync(rate);
        return NoContent();
    }

    /// <summary>
    /// Obtenir les statistiques des commissions
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var rates = await _commissionRateRepo.ListAllAsync();
        
        return Ok(new
        {
            TotalRates = rates.Count,
            ByDistributorType = rates.GroupBy(r => r.DistributorType)
                .Select(g => new { DistributorType = g.Key.ToString(), Count = g.Count() }),
            ByProductType = rates.GroupBy(r => r.ProductType)
                .Select(g => new { ProductType = g.Key.ToString(), Count = g.Count() }),
            AverageRate = rates.Any() ? rates.Average(r => r.Rate) : 0,
            AverageRatePercentage = rates.Any() ? rates.Average(r => r.Rate) * 100 : 0
        });
    }
}
