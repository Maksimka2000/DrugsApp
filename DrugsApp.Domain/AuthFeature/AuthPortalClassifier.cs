using System.Collections.Generic;
using System.Linq;

namespace DrugsApp.Domain.AuthFeature;

/// <summary>
/// Classifies Identity role sets for admin vs client portal separation.
/// </summary>
public static class AuthPortalClassifier
{
    /// <summary>
    /// Returns true when the user holds any admin-portal role.
    /// </summary>
    public static bool HasAnyAdminRole(IEnumerable<string> roles) =>
        roles.Any(role => AuthRoles.AdminRoleNames.Contains(role));

    /// <summary>
    /// Returns true when the user is a client-portal account without admin roles.
    /// </summary>
    public static bool IsClientPortalUser(IEnumerable<string> roles) =>
        (roles.Contains(AuthRoles.Client) || roles.Contains(AuthRoles.HealthcareProvider))
        && !HasAnyAdminRole(roles);

    /// <summary>
    /// Returns true when the user should authenticate through the admin portal.
    /// </summary>
    public static bool IsAdminPortalUser(IEnumerable<string> roles) =>
        HasAnyAdminRole(roles);
}
