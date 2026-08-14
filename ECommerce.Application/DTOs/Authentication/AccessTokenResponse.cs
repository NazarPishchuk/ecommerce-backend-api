namespace ECommerce.Application.DTOs.Authentication;

public sealed record AccessTokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);
