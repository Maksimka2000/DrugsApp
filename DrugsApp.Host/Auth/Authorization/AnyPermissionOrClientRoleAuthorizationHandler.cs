using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Enums;
using DrugsApp.Domain.AuthFeature.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DrugsApp.Host.Auth.Authorization;

/// <summary>
/// Authorization requirement satisfied by a specific client role or any listed permission.
/// </summary>
public sealed record AnyPermissionOrClientRoleRequirement(
    string Role,
    IReadOnlyCollection<Permission> Permissions) : IAuthorizationRequirement;

/// <summary>
/// Authorization handler that succeeds if the user has the client role or any given permission.
/// </summary>
public sealed class AnyPermissionOrClientRoleAuthorizationHandler(
    IUserPermissionService _userPermissionService) : AuthorizationHandler<AnyPermissionOrClientRoleRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionOrClientRoleRequirement requirement)
    {
        if (context.User.IsInRole(requirement.Role))
        {
            context.Succeed(requirement);
            return;
        }

        string? subject = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            context.User.FindFirstValue("sub");

        if (!Guid.TryParse(subject, out Guid userId))
        {
            return;
        }

        foreach (Permission permission in requirement.Permissions)
        {
            bool hasPermission = await _userPermissionService.HasPermissionAsync(
                userId,
                permission,
                CancellationToken.None);

            if (hasPermission)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
