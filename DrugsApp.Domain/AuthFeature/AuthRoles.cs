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
    /// Gets the admin-portal teacher role that designs virtual study clinics for classroom teaching.
    /// </summary>
    public const string Teacher = "Teacher";

    /// <summary>
    /// Gets the client-portal read-only role assigned to provisioned study doctor accounts.
    /// </summary>
    public const string StudyDoctor = "StudyDoctor";

    /// <summary>
    /// Gets the client-portal read-only role assigned to provisioned study patient accounts.
    /// </summary>
    public const string StudyPatient = "StudyPatient";

    /// <summary>
    /// Comma-separated member role names for ASP.NET Core authorize attributes.
    /// </summary>
    public const string MemberRoles = "Client,HealthcareProvider";

    /// <summary>
    /// Comma-separated role names allowed on read-only member endpoints, including the read-only study roles.
    /// </summary>
    public const string MemberReadRoles = "Client,HealthcareProvider,StudyDoctor,StudyPatient";

    /// <summary>
    /// Comma-separated role names allowed on read-only healthcare-provider supervision endpoints.
    /// </summary>
    public const string ProviderReadRoles = "HealthcareProvider,StudyDoctor";

    /// <summary>
    /// Client-portal roles for members using the consumer application.
    /// </summary>
    public static IReadOnlyCollection<string> MemberRoleNames => new[]
    {
        Client,
        HealthcareProvider,
        StudyDoctor,
        StudyPatient
    };

    /// <summary>
    /// Read-only client-portal roles provisioned exclusively through study mode.
    /// </summary>
    public static IReadOnlyCollection<string> StudyRoleNames => new[]
    {
        StudyDoctor,
        StudyPatient
    };

    /// <summary>
    /// Admin-portal roles that can be assigned through admin user management.
    /// </summary>
    public static IReadOnlyCollection<string> AdminRoleNames => new[]
    {
        ContentAdmin,
        CatalogAdmin,
        ImportAdmin,
        Admin,
        Teacher
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
        Admin,
        Teacher,
        StudyDoctor,
        StudyPatient
    };
}
