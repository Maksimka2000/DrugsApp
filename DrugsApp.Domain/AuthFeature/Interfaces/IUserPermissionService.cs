using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Enums;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Permission resolution and cache coordination for authenticated users.
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    /// Retrieves the set of permission names granted to the given user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of permission names.</returns>
    Task<IReadOnlyCollection<string>> GetPermissionNamesAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the user holds the specified permission.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="permission">Permission to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the permission is granted.</returns>
    Task<bool> HasPermissionAsync(Guid userId, Permission permission, CancellationToken cancellationToken);

    /// <summary>
    /// Primes the permission cache for a user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WarmAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Invalidates cached permissions for a user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InvalidateAsync(Guid userId, CancellationToken cancellationToken);
}
