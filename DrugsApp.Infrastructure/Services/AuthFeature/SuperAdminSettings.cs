namespace DrugsApp.Infrastructure.Services.AuthFeature;

public sealed class SuperAdminSettings
{
    public const string SectionName = "Auth:SuperAdmin";

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
