using System;
using Azure.Storage.Blobs;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.TestFeature.Repositories;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Domain.SystemTextFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.ExternalServices.BlobStorage;
using DrugsApp.Infrastructure.Repositories.ActiveSubstanceFeature;
using DrugsApp.Infrastructure.Repositories.ActiveSubstanceGroupFeature;
using DrugsApp.Infrastructure.Repositories.DrugFeature;
using DrugsApp.Infrastructure.Repositories.FoodFeature;
using DrugsApp.Infrastructure.Repositories.PharmaceuticalFormFeature;
using DrugsApp.Infrastructure.Repositories.RouteOfAdministrationFeature;
using DrugsApp.Infrastructure.Repositories.InteractionFeature;
using DrugsApp.Infrastructure.Repositories.TestFeature;
using DrugsApp.Infrastructure.Repositories.TempDrugFeature;
using DrugsApp.Infrastructure.Repositories.VetDrugFeature;
using DrugsApp.Infrastructure.Repositories.SupplementFeature;
using DrugsApp.Infrastructure.Repositories.SystemTextFeature;
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
        services.AddScoped<IDrugRepository, DrugRepository>();
        services.AddScoped<IDrugFormRepository, DrugFormRepository>();
        services.AddScoped<IPharmaceuticalFormRepository, PharmaceuticalFormRepository>();
        services.AddScoped<IRouteOfAdministrationRepository, RouteOfAdministrationRepository>();
        services.AddScoped<IActiveSubstanceRepository, ActiveSubstanceRepository>();
        services.AddScoped<IActiveSubstanceGroupRepository, ActiveSubstanceGroupRepository>();
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<IFoodGroupRepository, FoodGroupRepository>();
        services.AddScoped<IInteractionRepository, InteractionRepository>();
        services.AddScoped<ITempDrugRepository, TempDrugRepository>();
        services.AddScoped<ITempDrugImportBatchRepository, TempDrugImportBatchRepository>();
        services.AddScoped<IDefaultMappingForColumnsAttributeRepository, DefaultMappingForColumnsAttributeRepository>();
        services.AddScoped<ISupplementRepository, SupplementRepository>();
        services.AddScoped<ISupplementGroupRepository, SupplementGroupRepository>();
        services.AddScoped<ISystemTextRepository, SystemTextRepository>();
        services.AddHttpClient("ImportDownload", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        // Register BLOB AZURE External services here
        string? blobConnection = configuration.GetConnectionString("BlobStorage");
        if (string.IsNullOrWhiteSpace(blobConnection))
        {
            throw new InvalidOperationException("ConnectionStrings:BlobStorage must be configured.");
        }
        services.AddSingleton(_ => new BlobServiceClient(
            blobConnection,
            new BlobClientOptions(BlobClientOptions.ServiceVersion.V2021_12_02)));
        services.AddSingleton<IBlobService, BlobService>();


        return services;
    }
}
