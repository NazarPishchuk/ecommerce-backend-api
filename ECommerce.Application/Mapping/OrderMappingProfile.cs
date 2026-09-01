using AutoMapper;
using ECommerce.Application.DTOs.Orders.Admin;
using ECommerce.Application.DTOs.Orders.Customer;
using ECommerce.Application.DTOs.Orders.Seller;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, GetOrderListDto>();

        CreateMap<Order, GetOrderDto>();

        CreateMap<SellerOrder, GetSellerOrderDto>();

        CreateMap<OrderItem, GetOrderItemDto>();

        CreateMap<SellerOrder, GetSellerOrderListDto>()
            .ForMember(
                dest => dest.TotalAmount,
                opt => opt.MapFrom(src =>
                    src.OrderItems.Sum(x => x.UnitPrice * x.Quantity)))
            .ForMember(
                dest => dest.CreatedAt,
                opt => opt.MapFrom(src => src.Order.CreatedAt));

        CreateMap<SellerOrder, GetSellerOrderDetailsDto>()
            .ForMember(
                dest => dest.TotalAmount,
                opt => opt.MapFrom(src =>
                    src.OrderItems.Sum(x => x.UnitPrice * x.Quantity)))
            .ForMember(
                dest => dest.CreatedAt,
                opt => opt.MapFrom(src => src.Order.CreatedAt));

        CreateMap<Order, GetAdminOrderListDto>();

        CreateMap<Order, GetAdminOrderDto>();
    }
}