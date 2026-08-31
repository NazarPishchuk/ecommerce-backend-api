using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class SellerOrderConfiguration
    : IEntityTypeConfiguration<SellerOrder>
{
    public void Configure(
        EntityTypeBuilder<SellerOrder> builder)
    {
        builder.Property(x => x.StoreName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(x => x.Order)
            .WithMany(x => x.SellerOrders)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SellerProfile)
            .WithMany()
            .HasForeignKey(x => x.SellerProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}