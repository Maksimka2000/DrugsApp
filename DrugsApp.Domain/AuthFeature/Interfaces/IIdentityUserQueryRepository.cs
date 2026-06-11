using System;
using System.Threading;
using System.Threading.Tasks;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Read-only Identity user queries for admin and client user management screens.
/// </summary>
public interface IIdentityUserQueryRepository
{
    /// <summary>
    /// Searches admin-portal users with pagination and optional name or email filter.
    /// </summary>
    Task<IdentityAdminUserSearchResult> SearchAdminUsersAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads a single admin-portal user when present.
    /// </summary>
    Task<IdentityAdminUserRow?> GetAdminUserByIdAsync(
        Guid adminUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Searches client-portal users with pagination and optional email filter.
    /// </summary>
    Task<IdentityClientUserSearchResult> SearchClientUsersAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken);
}
