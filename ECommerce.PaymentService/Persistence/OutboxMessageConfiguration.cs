using ECommerce.PaymentService.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.PaymentService.Persistence;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.Type)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.RoutingKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Payload)
            .IsRequired();

        builder.Property(message => message.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex(message => new
        {
            message.ProcessedAtUtc,
            message.OccurredAtUtc
        });
    }
}