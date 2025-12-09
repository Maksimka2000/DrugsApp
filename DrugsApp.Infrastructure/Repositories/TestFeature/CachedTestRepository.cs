using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TestFeature.Entities;
using DrugsApp.Domain.TestFeature.Repositories;
using DrugsApp.Infrastructure.Common.Serialization;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace DrugsApp.Infrastructure.Repositories.TestFeature;

public sealed class CachedTestRepository : ITestRepository
{
    private readonly TestRepository _decorated;
    private readonly IDistributedCache _distributedCache;
    private readonly DrugsAppDbContext _dbContext;

    public CachedTestRepository(
        TestRepository decorated,
        IDistributedCache distributedCache,
        DrugsAppDbContext dbContext)
    {
        _decorated = decorated;
        _distributedCache = distributedCache;
        _dbContext = dbContext;
    }

   
    public async Task<Test?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {

        string key = $"test-{id}";

        // try to get member form the cache 
        string? cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
        
        Test? test = null;
        if (string.IsNullOrEmpty(cachedValue))
        {
            test = await _decorated.GetByIdAsync(id, cancellationToken);

            if (test is null){
                return test;
            }

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(test), cancellationToken);

            return test;
        }

        test = JsonConvert.DeserializeObject<Test>(
            cachedValue, 
            new JsonSerializerSettings{
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new PrivateResolver()
        });

        
        if (test is not null){
            _dbContext.Tests.Attach(test);
        }

        return test;
    }

    public async Task AddAsync(Test test, CancellationToken cancellationToken)
    {
        await _decorated.AddAsync(test, cancellationToken);
    }


    // private async Task RemoveAsync(Test test, CancellationToken cancellationToken)
    // {
    //     await _dbContext.Tests.RemoveAsync(test, cancellationToken);
    //     string cacheKey = $"test-{test.Id}";
    //     return _distributedCache.RemoveAsync(cacheKey, cancellationToken);
    // }
}

