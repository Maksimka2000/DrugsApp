using System.Reflection;
using DrugsApp.Domain.DrugFeature.DomainServices;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.Common.Behaviors;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.ApplicationServices;
using DrugsApp.Features.InteractionFeature.ApplicationServices;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddFeatures(
        this IServiceCollection services)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            // Register pipeline behaviors (ORDER MATTERS!)
            configuration.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });


        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Register applicaiton and domain services here
        services.AddScoped<ITempDrugReviewApplicationService, TempDrugReviewApplicationService>();
        services.AddScoped<ITempDrugUploadApplicationService, TempDrugUploadApplicationService>();
        services.AddScoped<ITempDrugImportApplicationService, TempDrugImportApplicationService>();
        services.AddScoped<ITempDrugRevertApplicationService, TempDrugRevertApplicationService>();
        services.AddScoped<IDrugReviewCanonicalizationService, DrugReviewCanonicalizationService>();
        services.AddScoped<IVetDrugCanonicalizationService, VetDrugCanonicalizationService>();
        services.AddScoped<IDrugSimilarityDomainService, DrugSimilarityDomainService>();
        services.AddScoped<IInteractionWriteApplicationService, InteractionWriteApplicationService>();
        services.AddScoped<IInteractionExpandedSyncApplicationService, InteractionExpandedSyncApplicationService>();
        services.AddScoped<IReproductiveHealthProfileApplicationService, ReproductiveHealthProfileApplicationService>();
        services.AddScoped<IHealthcareProviderProfileApplicationService, HealthcareProviderProfileApplicationService>();
        services.AddScoped<IHealthcareProviderTeardownService, HealthcareProviderTeardownService>();

        return services;
    }
}

