
using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;

internal class UserDTOToOrderResponseMappingProfile : Profile
{
    public UserDTOToOrderResponseMappingProfile()
    {
        CreateMap<UserDTO, OrderResponse>()
            .ForMember(dest => dest.UserPersonName, opt => opt.MapFrom(src => src.PersonName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
    }
}
