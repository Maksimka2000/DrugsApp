using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;

/// <summary>
/// Query-side contract for reading admin user lists and detail information.
/// </summary>
public interface IAdminUserManagementApplicationService
{
    /// <summary>
    /// Retrieves a paged list of admin users filtered by an optional search query.
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Maximum number of records to return.</param>
    /// <param name="search">Optional free-text search filter.</param>
    /// <param name="currentUserId">Identifier of the requesting admin user.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Paged list of admin users with total count.</returns>
    Task<ErrorOr<ListAdminUsersResponse>> ListAsync(
        int skip,
        int take,
        string? search,
        Guid currentUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves detail information for a specific admin user.
    /// </summary>
    /// <param name="adminUserId">Identifier of the admin user to retrieve.</param>
    /// <param name="currentUserId">Identifier of the requesting admin user.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Admin user details or a not-found error.</returns>
    Task<ErrorOr<AdminUserDetailDto>> GetByIdAsync(
        Guid adminUserId,
        Guid currentUserId,
        CancellationToken cancellationToken);
}
