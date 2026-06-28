using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Enums;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace DrugsApp.Infrastructure.Services.AuthFeature;

/// <summary>
/// Database-backed permission resolver for users.
/// </summary>
public sealed class UserPermissionServiceDb(
    UserManager<ApplicationUser> _userManager,
    RoleManager<ApplicationRole> _roleManager) : IUserPermissionService
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetPermissionNamesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Array.Empty<string>();
        }

        IList<string> roleNames = await _userManager.GetRolesAsync(user);
        if (roleNames.Count == 0)
        {
            return Array.Empty<string>();
        }

        var permissionNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (string roleName in roleNames)
        {
            ApplicationRole? role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (Claim claim in roleClaims)
            {
                if (claim.Type == PermissionClaimTypes.Permission)
                {
                    permissionNames.Add(claim.Value);
                }
            }
        }

        return permissionNames.ToArray();
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Permission permission,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<string> permissionNames = await GetPermissionNamesAsync(userId, cancellationToken);
        return permissionNames.Contains(permission.ToString(), StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public async Task WarmAsync(Guid userId, CancellationToken cancellationToken)
    {
        _ = await GetPermissionNamesAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public Task InvalidateAsync(Guid userId, CancellationToken cancellationToken) => Task.CompletedTask;
}
