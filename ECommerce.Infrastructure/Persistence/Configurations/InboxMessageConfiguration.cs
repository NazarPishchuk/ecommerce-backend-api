using ECommerce.Infrastructure.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public sealed class InboxMessageConfiguration
    : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.Type)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.ProcessedAtUtc)
            .IsRequired();
    }
}