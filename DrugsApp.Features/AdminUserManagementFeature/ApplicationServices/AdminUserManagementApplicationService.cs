using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;

/// <summary>
/// Query-side application service for listing and reading admin users with UI-friendly DTOs.
/// </summary>
public sealed class AdminUserManagementApplicationService(
    IIdentityUserQueryRepository _identityUserQueryRepository,
    ISuperAdminGuard _superAdminGuard) : IAdminUserManagementApplicationService
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
    public async Task<ErrorOr<ListAdminUsersResponse>> ListAsync(
        int skip,
        int take,
        string? search,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        string? normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        IdentityAdminUserSearchResult result = await _identityUserQueryRepository.SearchAdminUsersAsync(
            skip,
            take,
            normalizedSearch,
            cancellationToken);

        var items = result.Items
            .Select(row => MapListItem(row, currentUserId))
            .ToList();

        return new ListAdminUsersResponse(items, result.TotalCount);
    }

    /// <summary>
    /// Retrieves detail information for a specific admin user.
    /// </summary>
    /// <param name="adminUserId">Identifier of the admin user to retrieve.</param>
    /// <param name="currentUserId">Identifier of the requesting admin user.</param>
    /// <param name="cancellationToken">Propagation token to cancel the query.</param>
    /// <returns>Admin user details or a not-found error.</returns>
    public async Task<ErrorOr<AdminUserDetailDto>> GetByIdAsync(
        Guid adminUserId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        IdentityAdminUserRow? row = await _identityUserQueryRepository.GetAdminUserByIdAsync(
            adminUserId,
            cancellationToken);

        if (row is null)
        {
            return Error.NotFound(
                code: "Auth.AdminUserNotFound",
                description: "Admin user was not found.");
        }

        return MapDetail(row, currentUserId);
    }

    private AdminUserListItemDto MapListItem(IdentityAdminUserRow row, Guid currentUserId) =>
        new(
            Id: row.Id,
            FullName: row.FullName,
            Email: row.Email,
            AdminRole: row.AdminRole,
            Status: MapStatus(row.LockoutEnabled, row.LockoutEnd),
            CreatedAtUtc: row.CreatedAtUtc,
            IsProtected: _superAdminGuard.IsProtectedEmail(row.Email),
            IsCurrentUser: row.Id == currentUserId);

    private AdminUserDetailDto MapDetail(IdentityAdminUserRow row, Guid currentUserId) =>
        new(
            Id: row.Id,
            FullName: row.FullName,
            Email: row.Email,
            AdminRole: row.AdminRole,
            Status: MapStatus(row.LockoutEnabled, row.LockoutEnd),
            CreatedAtUtc: row.CreatedAtUtc,
            IsProtected: _superAdminGuard.IsProtectedEmail(row.Email),
            IsCurrentUser: row.Id == currentUserId);

    private static string MapStatus(bool lockoutEnabled, DateTimeOffset? lockoutEnd) =>
        lockoutEnabled && lockoutEnd.HasValue && lockoutEnd > DateTimeOffset.UtcNow
            ? "Suspended"
            : "Active";
}
