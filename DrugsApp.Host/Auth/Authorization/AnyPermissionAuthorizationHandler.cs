using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using DrugsApp.Domain.AuthFeature.Enums;
using DrugsApp.Domain.AuthFeature.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DrugsApp.Host.Auth.Authorization;

public sealed class AnyPermissionAuthorizationHandler(
    IUserPermissionService _userPermissionService) : AuthorizationHandler<AnyPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
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

            if (!hasPermission)
            {
                continue;
            }

            context.Succeed(requirement);
            return;
        }
    }
}
