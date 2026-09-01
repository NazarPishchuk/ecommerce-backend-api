using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class OrderRepository(
    ECommerceDbContext dbContext) : IOrderRepository
{
    public void Add(Order order)
    {
        dbContext.Orders.Add(order);
    }

    public Task<Order?> GetByIdAndUserIdAsync(int id, string userId, CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .AsNoTracking()
            .Include(x => x.SellerOrders)
                .ThenInclude(x => x.OrderItems)
            .FirstOrDefaultAsync(
            x => x.Id == id && x.UserId == userId,
            cancellationToken);
    }

    public Task<Order?> GetByIdAndUserIdForUpdateAsync(int id, string userId, CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(x => x.SellerOrders)
                .ThenInclude(x => x.OrderItems)
            .FirstOrDefaultAsync(
            x => x.Id == id && x.UserId == userId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TryCancelPendingSellerOrdersAsync(
        int orderId,
        string userId,
        CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.SellerOrders
            .Where(x =>
                x.OrderId == orderId &&
                x.Order.UserId == userId &&
                x.Status == SellerOrderStatus.Pending &&
                !dbContext.SellerOrders.Any(sellerOrder =>
                    sellerOrder.OrderId == orderId &&
                    sellerOrder.Status != SellerOrderStatus.Pending))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    x => x.Status,
                    SellerOrderStatus.Cancelled),
                cancellationToken);

        return affectedRows > 0;
    }
}
