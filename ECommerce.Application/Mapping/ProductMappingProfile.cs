using AutoMapper;
using ECommerce.Application.DTOs.Products;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, GetProductDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(
                dest => dest.StoreName,
                opt => opt.MapFrom(src => src.SellerProfile!.StoreName))
            .ForMember(
                dest => dest.StockStatus,
                opt => opt.MapFrom(src =>
                    src.Quantity == 0
                        ? StockStatus.OutOfStock
                        : src.Quantity <= 5
                            ? StockStatus.LowStock
                            : StockStatus.InStock));

        CreateMap<Product, GetProductListDto>() 
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(
                dest => dest.StoreName,
                opt => opt.MapFrom(src => src.SellerProfile!.StoreName))
            .ForMember(
                dest => dest.StockStatus,
                opt => opt.MapFrom(src =>
                    src.Quantity == 0
                        ? StockStatus.OutOfStock
                        : src.Quantity <= 5
                            ? StockStatus.LowStock
                            : StockStatus.InStock));

        CreateMap<Product, GetSellerProductDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(
                dest => dest.StockStatus,
                opt => opt.MapFrom(src =>
                    src.Quantity == 0
                        ? StockStatus.OutOfStock
                        : src.Quantity <= 5
                            ? StockStatus.LowStock
                            : StockStatus.InStock));

        CreateMap<CreateProductDto, Product>();

        CreateMap<UpdateProductDto, Product>();
    }
}