using AutoMapper;
using Ekiva.Application.DTOs;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ekiva.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReferenceDataController : ControllerBase
{
    private readonly IRepository<VehicleMake> _vehicleMakeRepo;
    private readonly IRepository<VehicleCategory> _vehicleCategoryRepo;
    private readonly IRepository<Currency> _currencyRepo;
    private readonly IRepository<ProfessionalCategory> _profCategoryRepo;
    private readonly IMapper _mapper;

    public ReferenceDataController(
        IRepository<VehicleMake> vehicleMakeRepo,
        IRepository<VehicleCategory> vehicleCategoryRepo,
        IRepository<Currency> currencyRepo,
        IRepository<ProfessionalCategory> profCategoryRepo,
        IMapper mapper)
    {
        _vehicleMakeRepo = vehicleMakeRepo;
        _vehicleCategoryRepo = vehicleCategoryRepo;
        _currencyRepo = currencyRepo;
        _profCategoryRepo = profCategoryRepo;
        _mapper = mapper;
    }

    [HttpGet("vehicle-makes")]
    public async Task<ActionResult<List<VehicleMakeDto>>> GetVehicleMakes()
    {
        var makes = await _vehicleMakeRepo.ListAllAsync();
        return Ok(_mapper.Map<List<VehicleMakeDto>>(makes));
    }

    [HttpGet("vehicle-categories")]
    public async Task<ActionResult<List<VehicleCategoryDto>>> GetVehicleCategories()
    {
        var categories = await _vehicleCategoryRepo.ListAllAsync();
        return Ok(_mapper.Map<List<VehicleCategoryDto>>(categories));
    }

    [HttpGet("currencies")]
    public async Task<ActionResult<List<CurrencyDto>>> GetCurrencies()
    {
        var currencies = await _currencyRepo.ListAllAsync();
        return Ok(_mapper.Map<List<CurrencyDto>>(currencies));
    }

    [HttpGet("professional-categories")]
    public async Task<ActionResult<List<ProfessionalCategoryDto>>> GetProfessionalCategories()
    {
        var categories = await _profCategoryRepo.ListAllAsync();
        return Ok(_mapper.Map<List<ProfessionalCategoryDto>>(categories));
    }
}
