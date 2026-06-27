using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.ClientUserManagementFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.ClientUserManagementFeature.ApplicationServices;

/// <summary>
/// Application service for listing client users.
/// </summary>
public sealed class ClientUserManagementApplicationService(
    IIdentityUserQueryRepository _identityUserQueryRepository) : IClientUserManagementApplicationService
{
    /// <summary>
    /// Lists client users with paging and optional search.
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="search">Optional search text.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged list response with total count.</returns>
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
