using AutoMapper;
using PISO.Entities.Models;
using PISO.Shared.DataTransferObjects;

namespace PISO.WebApp.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SystemConfig, PlansResponseDto>();
        CreateMap<PlanConfig, PlanConfigDto>();
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToDateTime()));
    }
}
