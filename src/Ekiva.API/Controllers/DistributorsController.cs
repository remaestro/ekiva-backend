using AutoMapper;
using Ekiva.Application.DTOs;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DistributorsController : ControllerBase
{
    private readonly IRepository<Distributor> _distributorRepo;
    private readonly IMapper _mapper;

    public DistributorsController(IRepository<Distributor> distributorRepo, IMapper mapper)
    {
        _distributorRepo = distributorRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtenir tous les distributeurs
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DistributorDto>>> GetAll()
    {
        var distributors = await _distributorRepo.ListAllAsync();
        return Ok(_mapper.Map<List<DistributorDto>>(distributors));
    }

    /// <summary>
    /// Rechercher des distributeurs avec pagination et filtres
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<object>> Search([FromBody] DistributorSearchDto searchDto)
    {
        var query = _distributorRepo.GetQueryable();

        // Filtre par terme de recherche
        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            var term = searchDto.SearchTerm.ToLower();
            query = query.Where(d => 
                d.Code.ToLower().Contains(term) ||
                d.Name.ToLower().Contains(term) ||
                d.Email.ToLower().Contains(term) ||
                d.PhoneNumber.Contains(term)
            );
        }

        // Filtre par type
        if (!string.IsNullOrWhiteSpace(searchDto.Type))
        {
            if (Enum.TryParse<DistributorType>(searchDto.Type, out var distributorType))
            {
                query = query.Where(d => d.Type == distributorType);
            }
        }

        // Filtre par statut actif
        if (searchDto.IsActive.HasValue)
        {
            query = query.Where(d => d.IsActive == searchDto.IsActive.Value);
        }

        // Pagination
        var total = await query.CountAsync();
        var distributors = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        return Ok(new
        {
            Data = _mapper.Map<List<DistributorDto>>(distributors),
            Total = total,
            Page = searchDto.PageNumber,
            PageSize = searchDto.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)searchDto.PageSize)
        });
    }

    /// <summary>
    /// Obtenir un distributeur par ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DistributorDto>> GetById(Guid id)
    {
        var distributor = await _distributorRepo.GetByIdAsync(id);
        if (distributor == null) return NotFound();
        return Ok(_mapper.Map<DistributorDto>(distributor));
    }

    /// <summary>
    /// Créer un nouveau distributeur
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DistributorDto>> Create(CreateDistributorDto createDto)
    {
        var distributor = _mapper.Map<Distributor>(createDto);
        
        // Générer un code unique
        distributor.Code = await GenerateDistributorCode(createDto.Type);
        
        await _distributorRepo.AddAsync(distributor);
        
        return CreatedAtAction(nameof(GetById), new { id = distributor.Id }, _mapper.Map<DistributorDto>(distributor));
    }

    /// <summary>
    /// Mettre à jour un distributeur
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<DistributorDto>> Update(Guid id, UpdateDistributorDto updateDto)
    {
        var distributor = await _distributorRepo.GetByIdAsync(id);
        if (distributor == null) return NotFound();

        _mapper.Map(updateDto, distributor);
        await _distributorRepo.UpdateAsync(distributor);

        return Ok(_mapper.Map<DistributorDto>(distributor));
    }

    /// <summary>
    /// Supprimer un distributeur
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var distributor = await _distributorRepo.GetByIdAsync(id);
        if (distributor == null) return NotFound();

        await _distributorRepo.DeleteAsync(distributor);
        return NoContent();
    }

    /// <summary>
    /// Activer/Désactiver un distributeur
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<DistributorDto>> ToggleStatus(Guid id)
    {
        var distributor = await _distributorRepo.GetByIdAsync(id);
        if (distributor == null) return NotFound();

        distributor.IsActive = !distributor.IsActive;
        await _distributorRepo.UpdateAsync(distributor);

        return Ok(_mapper.Map<DistributorDto>(distributor));
    }

    /// <summary>
    /// Obtenir les statistiques des distributeurs
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var distributors = await _distributorRepo.ListAllAsync();
        
        return Ok(new
        {
            Total = distributors.Count,
            Active = distributors.Count(d => d.IsActive),
            Inactive = distributors.Count(d => !d.IsActive),
            ByType = new
            {
                InternalAgent = distributors.Count(d => d.Type == DistributorType.InternalAgent),
                GeneralAgent = distributors.Count(d => d.Type == DistributorType.GeneralAgent),
                Broker = distributors.Count(d => d.Type == DistributorType.Broker),
                Bancassurance = distributors.Count(d => d.Type == DistributorType.Bancassurance)
            },
            RecentDistributors = distributors.OrderByDescending(d => d.CreatedAt).Take(5).Select(d => new
            {
                d.Id,
                d.Code,
                d.Name,
                d.Type,
                d.IsActive,
                d.CreatedAt
            })
        });
    }

    /// <summary>
    /// Obtenir les distributeurs actifs (pour les dropdowns)
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<DistributorDto>>> GetActiveDistributors()
    {
        var query = _distributorRepo.GetQueryable();
        var distributors = await query
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
        
        return Ok(_mapper.Map<List<DistributorDto>>(distributors));
    }

    private async Task<string> GenerateDistributorCode(string type)
    {
        var prefix = type switch
        {
            "InternalAgent" => "IA",
            "GeneralAgent" => "GA",
            "Broker" => "BR",
            "Bancassurance" => "BA",
            _ => "DT"
        };
        
        var distributors = await _distributorRepo.ListAllAsync();
        var count = distributors.Count(d => d.Code.StartsWith(prefix));
        
        return $"{prefix}-{(count + 1):D4}";
    }
}
