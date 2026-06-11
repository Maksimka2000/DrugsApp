using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.AuthFeature;

/// <summary>
/// EF-backed Identity user projections for admin and client management lists.
/// </summary>
public sealed class IdentityUserQueryRepository(DrugsAppDbContext _context) : IIdentityUserQueryRepository
{
    private static readonly string[] ClientPortalRoleNames =
    [
        AuthRoles.Client,
        AuthRoles.HealthcareProvider
    ];

    public async Task<IdentityAdminUserSearchResult> SearchAdminUsersAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken)
    {
        string? normalizedSearch = NormalizeSearch(search);
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;

        IQueryable<ApplicationUser> query = BuildAdminUsersQuery(normalizedSearch);

        int totalCount = await query.CountAsync(cancellationToken);

        List<ApplicationUser> users = await query
            .OrderBy(user => user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > utcNow)
            .ThenBy(user => user.FullName)
            .ThenBy(user => user.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        IReadOnlyList<IdentityAdminUserRow> items = await MapAdminUsersAsync(users, cancellationToken);

        return new IdentityAdminUserSearchResult(items, totalCount);
    }

    public async Task<IdentityAdminUserRow?> GetAdminUserByIdAsync(
        Guid adminUserId,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await BuildAdminUsersQuery(search: null)
            .SingleOrDefaultAsync(candidate => candidate.Id == adminUserId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        IReadOnlyList<IdentityAdminUserRow> rows = await MapAdminUsersAsync([user], cancellationToken);
        return rows.SingleOrDefault();
    }

    public async Task<IdentityClientUserSearchResult> SearchClientUsersAsync(
        int skip,
        int take,
        string? search,
        CancellationToken cancellationToken)
    {
        string? normalizedSearch = NormalizeSearch(search);
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;

        IQueryable<ApplicationUser> query = BuildClientUsersQuery(normalizedSearch);

        int totalCount = await query.CountAsync(cancellationToken);

        List<ApplicationUser> users = await query
            .OrderBy(user => user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > utcNow)
            .ThenBy(user => user.Email)
            .ThenBy(user => user.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        IReadOnlyList<IdentityClientUserRow> items = await MapClientUsersAsync(users, cancellationToken);

        return new IdentityClientUserSearchResult(items, totalCount);
    }

    private IQueryable<ApplicationUser> BuildAdminUsersQuery(string? search)
    {
        IQueryable<Guid> adminUserIds = from userRole in _context.UserRoles
                                        join role in _context.Roles on userRole.RoleId equals role.Id
                                        where AuthRoles.AdminRoleNames.Contains(role.Name!)
                                        select userRole.UserId;

        IQueryable<ApplicationUser> query = _context.Users
            .AsNoTracking()
            .Where(user => adminUserIds.Contains(user.Id));

        if (search is not null)
        {
            string pattern = $"%{search}%";
            query = query.Where(user =>
                EF.Functions.ILike(user.FullName ?? string.Empty, pattern)
                || EF.Functions.ILike(user.Email!, pattern));
        }

        return query;
    }

    private IQueryable<ApplicationUser> BuildClientUsersQuery(string? search)
    {
        IQueryable<Guid> adminUserIds = from userRole in _context.UserRoles
                                        join role in _context.Roles on userRole.RoleId equals role.Id
                                        where AuthRoles.AdminRoleNames.Contains(role.Name!)
                                        select userRole.UserId;

        IQueryable<Guid> clientUserIds = from userRole in _context.UserRoles
                                           join role in _context.Roles on userRole.RoleId equals role.Id
                                           where ClientPortalRoleNames.Contains(role.Name!)
                                           select userRole.UserId;

        IQueryable<ApplicationUser> query = _context.Users
            .AsNoTracking()
            .Where(user => clientUserIds.Contains(user.Id) && !adminUserIds.Contains(user.Id));

        if (search is not null)
        {
            string pattern = $"%{search}%";
            query = query.Where(user => EF.Functions.ILike(user.Email!, pattern));
        }

        return query;
    }

    private async Task<IReadOnlyList<IdentityAdminUserRow>> MapAdminUsersAsync(
        IReadOnlyList<ApplicationUser> users,
        CancellationToken cancellationToken)
    {
        if (users.Count == 0)
        {
            return Array.Empty<IdentityAdminUserRow>();
        }

        HashSet<Guid> userIds = users.Select(user => user.Id).ToHashSet();

        Dictionary<Guid, string> adminRolesByUserId = await (
            from userRole in _context.UserRoles.AsNoTracking()
            join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where userIds.Contains(userRole.UserId) && AuthRoles.AdminRoleNames.Contains(role.Name!)
            group role.Name! by userRole.UserId into grouped
            select new
            {
                UserId = grouped.Key,
                AdminRole = grouped.OrderBy(name => name).First()
            }).ToDictionaryAsync(
            row => row.UserId,
            row => row.AdminRole,
            cancellationToken);

        return users
            .Where(user => adminRolesByUserId.ContainsKey(user.Id))
            .Select(user => new IdentityAdminUserRow(
                Id: user.Id,
                FullName: user.FullName ?? string.Empty,
                Email: user.Email!,
                AdminRole: adminRolesByUserId[user.Id],
                LockoutEnabled: user.LockoutEnabled,
                LockoutEnd: user.LockoutEnd,
                CreatedAtUtc: user.CreatedAtUtc))
            .ToList();
    }

    private async Task<IReadOnlyList<IdentityClientUserRow>> MapClientUsersAsync(
        IReadOnlyList<ApplicationUser> users,
        CancellationToken cancellationToken)
    {
        if (users.Count == 0)
        {
            return Array.Empty<IdentityClientUserRow>();
        }

        HashSet<Guid> userIds = users.Select(user => user.Id).ToHashSet();

        List<(Guid UserId, string RoleName)> roleRows = await (
            from userRole in _context.UserRoles.AsNoTracking()
            join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where userIds.Contains(userRole.UserId) && ClientPortalRoleNames.Contains(role.Name!)
            select new ValueTuple<Guid, string>(userRole.UserId, role.Name!))
            .ToListAsync(cancellationToken);

        Dictionary<Guid, List<string>> rolesByUserId = roleRows
            .GroupBy(row => row.UserId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(row => row.RoleName).OrderBy(name => name).ToList());

        return users
            .Select(user => new IdentityClientUserRow(
                Id: user.Id,
                Email: user.Email!,
                Roles: rolesByUserId.TryGetValue(user.Id, out List<string>? roles)
                    ? roles
                    : Array.Empty<string>(),
                LockoutEnabled: user.LockoutEnabled,
                LockoutEnd: user.LockoutEnd,
                EmailConfirmed: user.EmailConfirmed,
                CreatedAtUtc: user.CreatedAtUtc))
            .ToList();
    }

    private static string? NormalizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        return search.Trim();
    }
}
