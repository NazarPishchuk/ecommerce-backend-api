using ECommerce.PaymentService.Domain;
using ECommerce.PaymentService.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.PaymentService.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PaymentDbContext).Assembly);
    }
}
