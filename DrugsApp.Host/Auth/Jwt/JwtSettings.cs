namespace DrugsApp.Host.Auth.Jwt;

/// <summary>
/// JWT configuration values for token issuing.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Configuration section key for JWT settings.
    /// </summary>
    /// <summary>
    /// Configuration section key for JWT settings.
    /// </summary>
    public const string SectionName = "Auth:Jwt";

    /// <summary>
    /// JWT issuer value.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// JWT audience value.
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Symmetric signing key for JWTs.
    /// </summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// Access token lifetime in minutes.
    /// </summary>
    public int AccessTokenMinutes { get; init; } = 60;

    /// <summary>
    /// Refresh token lifetime in days.
    /// </summary>
    public int RefreshTokenDays { get; init; } = 7;
}
