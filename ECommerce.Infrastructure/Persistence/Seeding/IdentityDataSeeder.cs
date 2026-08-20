using ECommerce.Application.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace ECommerce.Infrastructure.Persistence.Seeding;

public static class IdentityDataSeeder
{
    private static readonly string[] Roles = [
        AppRoles.Customer,
        AppRoles.Seller,
        AppRoles.Admin
    ];

    public static void Seed(DbContext context)
    {
        var roles = context.Set<IdentityRole>();

        foreach (var roleName in Roles)
        {
            var normalizedName = roleName.ToUpperInvariant();

            if (roles.Any(role => role.NormalizedName == normalizedName))
            {
                continue;
            }

            roles.Add(new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                NormalizedName = normalizedName,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
        }

        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var roles = context.Set<IdentityRole>();

        foreach (var roleName in Roles)
        {
            var normalizedName = roleName.ToUpperInvariant();

            if (await roles.AnyAsync(
                    role => role.NormalizedName == normalizedName,
                    cancellationToken))
            {
                continue;
            }

            roles.Add(new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                NormalizedName = normalizedName,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
