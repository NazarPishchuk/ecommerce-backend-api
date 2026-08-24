using AutoMapper;
using ECommerce.Application.DTOs.Categories;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, GetCategoryDto>();

        CreateMap<CreateCategoryDto, Category>()
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(
                dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name.Trim().ToUpperInvariant()));

        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(
                dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name.Trim().ToUpperInvariant()));
    }
}
