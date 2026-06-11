using System.Collections.Generic;

namespace DrugsApp.Domain.AuthFeature;

/// <summary>
/// Defines the supported role names for the authorization model.
/// </summary>
public static class AuthRoles
{
    /// <summary>
    /// Gets the default client role.
    /// </summary>
    public const string Client = "Client";

    /// <summary>
    /// Gets the healthcare-provider role.
    /// </summary>
    public const string HealthcareProvider = "HealthcareProvider";

    /// <summary>
    /// Gets the content-admin role.
    /// </summary>
    public const string ContentAdmin = "ContentAdmin";

    /// <summary>
    /// Gets the catalog-admin role.
    /// </summary>
    public const string CatalogAdmin = "CatalogAdmin";

    /// <summary>
    /// Gets the import-admin role.
    /// </summary>
    public const string ImportAdmin = "ImportAdmin";

    /// <summary>
    /// Gets the full-admin role.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Client-portal roles for members using the consumer application.
    /// </summary>
    public static IReadOnlyCollection<string> MemberRoleNames => new[]
    {
        Client,
        HealthcareProvider
    };

    /// <summary>
    /// Admin-portal roles that can be assigned through admin user management.
    /// </summary>
    public static IReadOnlyCollection<string> AdminRoleNames => new[]
    {
        ContentAdmin,
        CatalogAdmin,
        ImportAdmin,
        Admin
    };

    /// <summary>
    /// Returns all supported role names.
    /// </summary>
    public static IReadOnlyCollection<string> All => new[]
    {
        Client,
        HealthcareProvider,
        ContentAdmin,
        CatalogAdmin,
        ImportAdmin,
        Admin
    };
}
