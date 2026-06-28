using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace DrugsApp.Infrastructure.Services.AuthFeature;

/// <summary>
/// Identity role reader for user role membership queries.
/// </summary>
public sealed class UserRoleReadService(UserManager<ApplicationUser> userManager) : IUserRoleReadService
{
    /// <inheritdoc />
    public async Task<bool> IsInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        return await userManager.IsInRoleAsync(user, roleName);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Array.Empty<string>();
        }

        IList<string> roleNames = await userManager.GetRolesAsync(user);
        return roleNames.ToArray();
    }
}
