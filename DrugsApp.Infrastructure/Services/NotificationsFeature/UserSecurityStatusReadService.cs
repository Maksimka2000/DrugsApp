using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence.Identity;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace DrugsApp.Infrastructure.Services.NotificationsFeature;

/// <summary>
/// Reads user security status flags from Identity.
/// </summary>
public sealed class UserSecurityStatusReadService(UserManager<ApplicationUser> _userManager)
    : IUserSecurityStatusReadService
{
    /// <inheritdoc />
    public async Task<ErrorOr<UserSecurityStatusSnapshot>> GetStatusAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "UserSecurityStatus.UserNotFound",
                description: "User was not found.",
                metadata: new()
                {
                    ["resourceType"] = "ApplicationUser",
                    ["resourceId"] = userId.ToString()
                });
        }

        return new UserSecurityStatusSnapshot(
            EmailConfirmed: user.EmailConfirmed,
            TwoFactorEnabled: user.TwoFactorEnabled);
    }
}
