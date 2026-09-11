using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SystemTextFeature.Entities;
using DrugsApp.Domain.SystemTextFeature.Repositories;
using DrugsApp.Infrastructure.Common.Serialization;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace DrugsApp.Infrastructure.Repositories.SystemTextFeature;

/// <summary>
/// Cached decorator over <see cref="ISystemTextRepository"/> for system text lookups.
/// </summary>
public sealed class CachedSystemTextRepository(
    ISystemTextRepository _decorated,
    IDistributedCache _distributedCache,
    DrugsAppDbContext _dbContext) : ISystemTextRepository
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
    };

    /// <inheritdoc />
    public async Task<SystemText?> GetByKeyAsync(string key, CancellationToken cancellationToken)
    {
        string cacheKey = GetCacheKey(key);
        string? cachedValue = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedValue))
        {
            SystemText? cachedSystemText = JsonConvert.DeserializeObject<SystemText>(
                cachedValue,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    ContractResolver = new PrivateResolver()
                });

            if (cachedSystemText is not null)
            {
                _dbContext.Set<SystemText>().Attach(cachedSystemText);
                return cachedSystemText;
            }
        }

        SystemText? systemText = await _decorated.GetByKeyAsync(key, cancellationToken);
        if (systemText is null)
        {
            return null;
        }

        await _distributedCache.SetStringAsync(
            cacheKey,
            JsonConvert.SerializeObject(systemText),
            CacheOptions,
            cancellationToken);

        return systemText;
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(string key, CancellationToken cancellationToken)
    {
        await _distributedCache.RemoveAsync(GetCacheKey(key), cancellationToken);
        await _decorated.InvalidateAsync(key, cancellationToken);
    }

    private static string GetCacheKey(string key) => $"systemtext:{key}";
}
