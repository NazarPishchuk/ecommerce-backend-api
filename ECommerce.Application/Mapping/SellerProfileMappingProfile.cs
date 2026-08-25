using AutoMapper;
using ECommerce.Application.DTOs.Sellers;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping;

public class SellerProfileMappingProfile : Profile
{
    public SellerProfileMappingProfile()
    {
        CreateMap<SellerProfile, GetSellerProfileDto>();

        CreateMap<SellerProfile, GetSellerProfileListDto>();

        CreateMap<CreateSellerProfileDto, SellerProfile>()
            .ForMember(
                dest => dest.StoreName,
                opt => opt.MapFrom(src => src.StoreName.Trim()))
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description.Trim()));

        CreateMap<UpdateSellerProfileDto, SellerProfile>()
            .ForMember(
                dest => dest.StoreName,
                opt => opt.MapFrom(src => src.StoreName.Trim()))
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => src.Description.Trim()));
    }
}
