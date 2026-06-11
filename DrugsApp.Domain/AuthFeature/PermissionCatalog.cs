using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.AuthFeature.Enums;

namespace DrugsApp.Domain.AuthFeature;

/// <summary>
/// Defines the role-to-permission mapping used to seed authorization data.
/// </summary>
public static class PermissionCatalog
{
    private static readonly Permission[] ClientPortalSearchPermissions =
    [
        Permission.SearchCatalog,
        Permission.SearchReferenceData,
        Permission.SearchHealthReference,
    ];

    /// <summary>
    /// Returns the permissions assigned to each supported role.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyCollection<Permission>> RolePermissions => new Dictionary<string, IReadOnlyCollection<Permission>>
    {
        [AuthRoles.Client] = ClientPortalSearchPermissions,
        [AuthRoles.HealthcareProvider] = ClientPortalSearchPermissions
            .Append(Permission.ManagePatientSupervision)
            .ToArray(),
        [AuthRoles.ContentAdmin] = new[]
        {
            Permission.ManageWebsiteContent
        },
        [AuthRoles.CatalogAdmin] = new[]
        {
            Permission.ManageCatalog,
            Permission.ManageInteractions,
            Permission.ManageReferenceData,
            Permission.ManageOther
        },
        [AuthRoles.ImportAdmin] = new[]
        {
            Permission.ManageImports,
            Permission.ManageReviews
        },
        [AuthRoles.Admin] = new[]
        {
            Permission.SearchCatalog,
            Permission.SearchReferenceData,
            Permission.SearchHealthReference,
            Permission.ManagePatientSupervision,
            Permission.ManageCatalog,
            Permission.ManageInteractions,
            Permission.ManageReferenceData,
            Permission.ManageImports,
            Permission.ManageReviews,
            Permission.ManageOther,
            Permission.ManageWebsiteContent,
            Permission.ManageClientUsers,
            Permission.ManageAdminUsers,
            Permission.ManageRoles
        }
    };
}
