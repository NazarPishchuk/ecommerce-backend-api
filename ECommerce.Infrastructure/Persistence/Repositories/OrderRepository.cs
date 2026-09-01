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

    public async Task<bool> TryCancelSellerOrdersAsync(
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

    public async Task<IReadOnlyList<SellerOrder>> GetSellerOrdersByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.SellerOrders
            .AsNoTracking()
            .Include(x => x.Order)
            .Include(x => x.OrderItems)
            .Where(x =>
                x.SellerProfile != null &&
                x.SellerProfile.UserId == userId)
            .OrderByDescending(x => x.Order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SellerOrder?> GetSellerOrderByIdAndUserIdAsync(
        int id,
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.SellerOrders
            .AsNoTracking()
            .Include(x => x.Order)
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == id &&
                    x.SellerProfile != null &&
                    x.SellerProfile.UserId == userId,
                cancellationToken);
    }

    public async Task<bool> TryConfirmSellerOrderAsync(
        int id,
        string userId,
        CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.SellerOrders
            .Where(x => x.Id == id &&
                   x.SellerProfile!.UserId == userId &&
                   x.Status == SellerOrderStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    x => x.Status,
                    SellerOrderStatus.Confirmed),
                cancellationToken);

        return affectedRows == 1;
    }

    public async Task RecalculateOrderStatusAsync(
    int orderId,
    CancellationToken cancellationToken)
    {
        await dbContext.Orders
            .Where(order =>
                order.Id == orderId &&
                order.Status != OrderStatus.Cancelled)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    order => order.Status,
                    order =>
                        !dbContext.SellerOrders.Any(sellerOrder =>
                            sellerOrder.OrderId == orderId &&
                            sellerOrder.Status != SellerOrderStatus.Pending)
                                ? OrderStatus.Pending

                        : !dbContext.SellerOrders.Any(sellerOrder =>
                            sellerOrder.OrderId == orderId &&
                            sellerOrder.Status != SellerOrderStatus.Rejected)
                                ? OrderStatus.Rejected

                        : !dbContext.SellerOrders.Any(sellerOrder =>
                            sellerOrder.OrderId == orderId &&
                            sellerOrder.Status != SellerOrderStatus.Delivered &&
                            sellerOrder.Status != SellerOrderStatus.Rejected)
                                ? OrderStatus.Completed

                        : OrderStatus.Processing),
                cancellationToken);
    }

    public async Task<bool> TryRejectSellerOrderAsync(
        int id,
        string userId,
        CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.SellerOrders
            .Where(x =>
                x.Id == id &&
                x.SellerProfile!.UserId == userId &&
                x.Status == SellerOrderStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    x => x.Status,
                    SellerOrderStatus.Rejected),
                cancellationToken);

        return affectedRows == 1;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

