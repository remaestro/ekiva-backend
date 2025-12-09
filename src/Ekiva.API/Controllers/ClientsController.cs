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
public class ClientsController : ControllerBase
{
    private readonly IRepository<Client> _clientRepo;
    private readonly IMapper _mapper;

    public ClientsController(IRepository<Client> clientRepo, IMapper mapper)
    {
        _clientRepo = clientRepo;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClientDto>>> GetAll()
    {
        var clients = await _clientRepo.ListAllAsync();
        return Ok(_mapper.Map<List<ClientDto>>(clients));
    }

    [HttpPost("search")]
    public async Task<ActionResult<object>> Search([FromBody] ClientSearchDto searchDto)
    {
        var query = _clientRepo.GetQueryable();

        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            var term = searchDto.SearchTerm.ToLower();
            query = query.Where(c => 
                c.ReferenceNumber.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                c.PhoneNumber.Contains(term) ||
                (c.FirstName != null && c.FirstName.ToLower().Contains(term)) ||
                (c.LastName != null && c.LastName.ToLower().Contains(term)) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(term))
            );
        }

        if (!string.IsNullOrWhiteSpace(searchDto.Type))
        {
            if (Enum.TryParse<ClientType>(searchDto.Type, out var clientType))
            {
                query = query.Where(c => c.Type == clientType);
            }
        }

        if (!string.IsNullOrWhiteSpace(searchDto.City))
        {
            query = query.Where(c => c.City.ToLower() == searchDto.City.ToLower());
        }

        var total = await query.CountAsync();
        var clients = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        return Ok(new
        {
            Data = _mapper.Map<List<ClientDto>>(clients),
            Total = total,
            Page = searchDto.PageNumber,
            PageSize = searchDto.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)searchDto.PageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        var client = await _clientRepo.GetByIdAsync(id);
        if (client == null) return NotFound();
        return Ok(_mapper.Map<ClientDto>(client));
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create(CreateClientDto createDto)
    {
        var client = _mapper.Map<Client>(createDto);
        
        client.ReferenceNumber = await GenerateReferenceNumber();
        
        await _clientRepo.AddAsync(client);
        
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, _mapper.Map<ClientDto>(client));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClientDto>> Update(Guid id, UpdateClientDto updateDto)
    {
        var client = await _clientRepo.GetByIdAsync(id);
        if (client == null) return NotFound();

        _mapper.Map(updateDto, client);
        await _clientRepo.UpdateAsync(client);

        return Ok(_mapper.Map<ClientDto>(client));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var client = await _clientRepo.GetByIdAsync(id);
        if (client == null) return NotFound();

        await _clientRepo.DeleteAsync(client);
        return NoContent();
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var clients = await _clientRepo.ListAllAsync();
        
        return Ok(new
        {
            Total = clients.Count,
            Individual = clients.Count(c => c.Type == ClientType.Individual),
            Company = clients.Count(c => c.Type == ClientType.Company),
            RecentClients = clients.OrderByDescending(c => c.CreatedAt).Take(5).Select(c => new
            {
                c.Id,
                c.ReferenceNumber,
                c.FullName,
                c.Type,
                c.CreatedAt
            })
        });
    }

    private async Task<string> GenerateReferenceNumber()
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        
        var clients = await _clientRepo.ListAllAsync();
        var count = clients.Count(c => c.CreatedAt.Year == year && c.CreatedAt.Month == month);
        
        return $"CL-{year}{month:D2}-{(count + 1):D4}";
    }
}
