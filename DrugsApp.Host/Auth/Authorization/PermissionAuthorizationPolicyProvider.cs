using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DrugsApp.Host.Auth.Authorization;

/// <summary>
/// Builds dynamic authorization policies for permission-based requirements.
/// </summary>
public sealed class PermissionAuthorizationPolicyProvider(
    IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    /// <summary>
    /// Resolves authorization policies for permission and composite requirements.
    /// </summary>
    /// <param name="policyName">Requested policy name.</param>
    /// <returns>Authorization policy or null when not applicable.</returns>
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? existingPolicy = await base.GetPolicyAsync(policyName);
        if (existingPolicy is not null)
        {
            return existingPolicy;
        }

        if (policyName.StartsWith("AnyPermissionOrClient:", StringComparison.Ordinal))
        {
            string payload = policyName["AnyPermissionOrClient:".Length..];
            string[] segments = payload.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length < 2)
            {
                return null;
            }

            string role = segments[0];
            Permission[] permissions = segments
                .Skip(1)
                .Select(permissionName => Enum.Parse<Permission>(permissionName))
                .ToArray();

            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new AnyPermissionOrClientRoleRequirement(role, permissions))
                .Build();
        }

        if (policyName.StartsWith("AnyPermission:", StringComparison.Ordinal))
        {
            string[] permissionNames = policyName["AnyPermission:".Length..]
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            Permission[] permissions = permissionNames
                .Select(permissionName => Enum.Parse<Permission>(permissionName))
                .ToArray();

            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new AnyPermissionRequirement(permissions))
                .Build();
        }

        if (!Enum.TryParse(policyName, ignoreCase: false, out Permission permission))
        {
            return null;
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();
    }
}

/// <summary>
/// Requirement for a specific permission.
/// </summary>
/// <param name="Permission">Permission to enforce.</param>
public sealed record PermissionRequirement(Permission Permission) : IAuthorizationRequirement;

/// <summary>
/// Requirement that succeeds when any provided permission is granted.
/// </summary>
/// <param name="Permissions">Allowed permissions.</param>
public sealed record AnyPermissionRequirement(IReadOnlyCollection<Permission> Permissions)
    : IAuthorizationRequirement;
