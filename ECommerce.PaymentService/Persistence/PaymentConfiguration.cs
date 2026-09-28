using ECommerce.PaymentService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.PaymentService.Persistence;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.ProviderPaymentId)
            .HasMaxLength(200);

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.ProviderPaymentId)
            .IsUnique()
            .HasFilter("[ProviderPaymentId] IS NOT NULL");
    }
}
