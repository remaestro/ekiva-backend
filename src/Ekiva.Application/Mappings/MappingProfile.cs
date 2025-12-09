using AutoMapper;
using Ekiva.Application.DTOs;
using Ekiva.Core.Entities;

namespace Ekiva.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Vehicle Mappings
        CreateMap<VehicleMake, VehicleMakeDto>();
        CreateMap<VehicleModel, VehicleModelDto>()
            .ForMember(dest => dest.MakeName, opt => opt.MapFrom(src => src.Make.Name));
        CreateMap<VehicleCategory, VehicleCategoryDto>();

        // Client Mappings
        CreateMap<Client, ClientDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        CreateMap<CreateClientDto, Client>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<ClientType>(src.Type)));

        // Distributor Mappings
        CreateMap<Distributor, DistributorDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        // Reference Data Mappings
        CreateMap<Currency, CurrencyDto>();
        CreateMap<ProfessionalCategory, ProfessionalCategoryDto>();

        // Commission & Tax Mappings (Phase 8)
        CreateMap<CommissionRate, CommissionRateDto>()
            .ForMember(dest => dest.DistributorType, opt => opt.MapFrom(src => src.DistributorType.ToString()))
            .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.ToString()));

        CreateMap<ProductTaxRate, ProductTaxRateDto>()
            .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.ToString()));
    }
}
