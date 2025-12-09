using System;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.TestFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Repositories.TestFeature;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DrugsApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<DrugsAppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres")));

        // regis cache registration
        string? redisConnection = configuration.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
        });

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories here
        services.AddScoped<ITestRepository, TestRepository>();
        services.Decorate<ITestRepository, CachedTestRepository>();

        // Register External services here


        return services;
    }
}
