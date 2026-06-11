using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.ClientUserManagementFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.ClientUserManagementFeature.ApplicationServices;

public sealed class ClientUserManagementApplicationService(
    IIdentityUserQueryRepository _identityUserQueryRepository) : IClientUserManagementApplicationService
{
    public async Task<ErrorOr<ListClientUsersResponse>> ListAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken)
    {
        string? normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        IdentityClientUserSearchResult result = await _identityUserQueryRepository.SearchClientUsersAsync(
            skip,
            take,
            normalizedSearch,
            cancellationToken);

        var items = result.Items
            .Select(row => new ClientUserListItemDto(
                Id: row.Id,
                Email: row.Email,
                Roles: row.Roles,
                Status: MapStatus(row.LockoutEnabled, row.LockoutEnd),
                IsEmailVerified: row.EmailConfirmed,
                CreatedAtUtc: row.CreatedAtUtc))
            .ToList();

        return new ListClientUsersResponse(items, result.TotalCount);
    }

    private static string MapStatus(bool lockoutEnabled, DateTimeOffset? lockoutEnd) =>
        lockoutEnabled && lockoutEnd.HasValue && lockoutEnd > DateTimeOffset.UtcNow
            ? "Suspended"
            : "Active";
}
