using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using DrugsApp.Domain.AuthFeature.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DrugsApp.Host.Auth.Authorization;

public sealed class PermissionAuthorizationHandler(
    IUserPermissionService _userPermissionService) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        string? subject = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            context.User.FindFirstValue("sub");

        if (!Guid.TryParse(subject, out Guid userId))
        {
            return;
        }

        bool hasPermission = await _userPermissionService.HasPermissionAsync(
            userId,
            requirement.Permission,
            CancellationToken.None);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
