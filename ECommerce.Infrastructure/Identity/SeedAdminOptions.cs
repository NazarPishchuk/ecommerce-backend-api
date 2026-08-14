namespace ECommerce.Infrastructure.Identity;

public sealed class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    public required string Email { get; init; }
    public required string  Password { get; init; }

    public string FirstName { get; init; } = "System";
    public string LastName { get; init; } = "Admin";
}
