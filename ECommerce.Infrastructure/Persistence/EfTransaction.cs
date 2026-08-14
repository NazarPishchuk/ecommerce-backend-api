using ECommerce.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Infrastructure.Persistence;

internal sealed class EfTransaction(IDbContextTransaction transaction) : ITransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return transaction.CommitAsync(cancellationToken);
    }

    public Task RollbackAsync()
    {
        return transaction.RollbackAsync();
    }

    public ValueTask DisposeAsync()
    {
        return transaction.DisposeAsync();
    }
}
