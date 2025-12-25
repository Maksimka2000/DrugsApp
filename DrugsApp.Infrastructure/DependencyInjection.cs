using System;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Domain.TestFeature.Repositories;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Repositories.ActiveSubstanceFeature;
using DrugsApp.Infrastructure.Repositories.PharmaceuticalFormFeature;
using DrugsApp.Infrastructure.Repositories.RouteOfAdministrationFeature;
using DrugsApp.Infrastructure.Repositories.TestFeature;
using DrugsApp.Infrastructure.Repositories.VetDrugFeature;
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
        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories here
        services.AddScoped<ITestRepository, TestRepository>();
        services.Decorate<ITestRepository, CachedTestRepository>();
        services.AddScoped<IVetDrugRepository, VetDrugRepository>();
        services.AddScoped<IPharmaceuticalFormRepository, PharmaceuticalFormRepository>();
        services.AddScoped<IRouteOfAdministrationRepository, RouteOfAdministrationRepository>();
        services.AddScoped<IActiveSubstanceRepository, ActiveSubstanceRepository>();

        // Register External services here


        return services;
    }
}
