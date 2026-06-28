namespace DrugsApp.Infrastructure.Services.AuthFeature;

/// <summary>
/// Configuration settings for bootstrap super-admin account.
/// </summary>
public sealed class SuperAdminSettings
{
    /// <summary>
    /// Configuration section path for super-admin settings.
    /// </summary>
    public const string SectionName = "Auth:SuperAdmin";

    /// <summary>
    /// Super-admin email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Super-admin initial password.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
