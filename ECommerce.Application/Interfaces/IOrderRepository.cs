using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IOrderRepository
{
    void Add(Order order);
}
