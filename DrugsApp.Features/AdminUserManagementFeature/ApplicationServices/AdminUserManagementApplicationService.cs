using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;

public sealed class AdminUserManagementApplicationService(
    IIdentityUserQueryRepository _identityUserQueryRepository,
    ISuperAdminGuard _superAdminGuard) : IAdminUserManagementApplicationService
{
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
