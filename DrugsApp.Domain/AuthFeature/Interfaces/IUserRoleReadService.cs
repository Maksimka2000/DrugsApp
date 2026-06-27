using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Read-only role queries for Identity users.
/// </summary>
public interface IUserRoleReadService
{
    /// <summary>
    /// Checks if the given user belongs to the specified role.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="roleName">Role name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the user is in the role.</returns>
    Task<bool> IsInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the role names assigned to the user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of role names.</returns>
    Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken);
}
