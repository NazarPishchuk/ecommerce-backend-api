using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class OrderRepository(
    ECommerceDbContext dbContext) : IOrderRepository
{
    public void Add(Order order)
    {
        dbContext.Orders.Add(order);
    }
}
