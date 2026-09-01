using AutoMapper;
using ECommerce.Application.DTOs.Orders;
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
    }
}