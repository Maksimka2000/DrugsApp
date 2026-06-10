using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Enums;
using DrugsApp.Domain.AuthFeature.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DrugsApp.Infrastructure.Services.AuthFeature;

public sealed class UserPermissionServiceCached(
    IUserPermissionService _decorated,
    IDistributedCache _distributedCache) : IUserPermissionService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    public async Task<IReadOnlyCollection<string>> GetPermissionNamesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        string cacheKey = GetCacheKey(userId);
        string? cachedValue = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedValue))
        {
            string[]? permissionNames = JsonSerializer.Deserialize<string[]>(cachedValue);
            if (permissionNames is not null)
            {
                return permissionNames;
            }
        }

        IReadOnlyCollection<string> permissionNamesFromDatabase = await _decorated.GetPermissionNamesAsync(
            userId,
            cancellationToken);

        await _distributedCache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(permissionNamesFromDatabase),
            CacheOptions,
            cancellationToken);

        return permissionNamesFromDatabase;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Permission permission,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<string> permissionNames = await GetPermissionNamesAsync(userId, cancellationToken);
        return permissionNames.Contains(permission.ToString(), StringComparer.Ordinal);
    }

    public async Task WarmAsync(Guid userId, CancellationToken cancellationToken)
    {
        _ = await GetPermissionNamesAsync(userId, cancellationToken);
    }

    public async Task InvalidateAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _distributedCache.RemoveAsync(GetCacheKey(userId), cancellationToken);
        await _decorated.InvalidateAsync(userId, cancellationToken);
    }

    private static string GetCacheKey(Guid userId) => $"permissions:{userId}";
}
