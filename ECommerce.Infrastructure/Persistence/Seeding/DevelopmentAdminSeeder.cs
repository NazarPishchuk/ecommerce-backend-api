using ECommerce.Application.Authorization;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Persistence.Seeding;

public class DevelopmentAdminSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<SeedAdminOptions> options)
{
    private readonly SeedAdminOptions _options = options.Value;

    public async Task SeedAsync()
    {
        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
        {
            throw new InvalidOperationException(
                "Admin role does not exist. Apply database seeding first.");
        }

        var user = await userManager.FindByEmailAsync(_options.Email);

        if(user is null)
        {
            user = new ApplicationUser
            {
                UserName = _options.Email,
                Email = _options.Email,
                FirstName = _options.FirstName,
                LastName = _options.LastName,
                EmailConfirmed = true
            };

            var createResult =
                await userManager.CreateAsync(user, _options.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Failed to seed admin user: {errors}");
            }
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;

            var updateResult =
                await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to confirm seeded admin email.");
            }
        }

        if (!await userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            var roleResult =
                await userManager.AddToRoleAsync(user, AppRoles.Admin);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to assign Admin role.");
            }
        }
    }
}
