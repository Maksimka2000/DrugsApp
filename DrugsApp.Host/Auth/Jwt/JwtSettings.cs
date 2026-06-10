namespace DrugsApp.Host.Auth.Jwt;

public sealed class JwtSettings
{
    public const string SectionName = "Auth:Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 60;

    public int RefreshTokenDays { get; init; } = 7;
}
