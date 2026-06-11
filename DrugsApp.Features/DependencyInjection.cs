using System.Reflection;
using DrugsApp.Domain.DrugFeature.DomainServices;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.Common.Behaviors;
using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.ApplicationServices;
using DrugsApp.Features.InteractionFeature.ApplicationServices;
using DrugsApp.Features.MealTimeManagementFeature.ApplicationServices;
using DrugsApp.Features.NotificationsFeature.ApplicationServices;
using DrugsApp.Features.RegularDrugsFeature.ApplicationServices;
using DrugsApp.Features.RegularFoodFeature.ApplicationServices;
using DrugsApp.Features.RegularSupplementsFeature.ApplicationServices;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ApplicationServices;
using DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;
using DrugsApp.Features.AdminUserManagementFeature.ApplicationServices;
using DrugsApp.Features.ClientUserManagementFeature.ApplicationServices;
using DrugsApp.Domain.AccountFeature.Interfaces;
using DrugsApp.Features.AccountFeature.ApplicationServices;
using DrugsApp.Features.DrugInteractionToolFeature.ApplicationServices;
using DrugsApp.Features.DrugToolFeature.ApplicationServices;
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
        services.AddScoped<IClientMealTimeNormalizationService, ClientMealTimeNormalizationService>();
        services.AddScoped<INotificationProjectionService, NotificationProjectionApplicationService>();
        services.AddScoped<INotificationDetailHydrationApplicationService, NotificationDetailHydrationApplicationService>();
        services.AddScoped<IRegularDrugsApplicationService, RegularDrugsApplicationService>();
        services.AddScoped<IRegularFoodExpirationApplicationService, RegularFoodExpirationApplicationService>();
        services.AddScoped<IRegularSupplementExpirationApplicationService, RegularSupplementExpirationApplicationService>();
        services.AddScoped<IClientOwnSupplementDeletionApplicationService, ClientOwnSupplementDeletionApplicationService>();
        services.AddScoped<IMedicalChangeProposalApplyService, MedicalChangeProposalApplyService>();
        services.AddScoped<IMedicalChangeProposalDraftValidationService, MedicalChangeProposalDraftValidationService>();
        services.AddScoped<IProviderPatientMedicalSummaryReadService, ProviderPatientMedicalSummaryReadService>();
        services.AddScoped<IAdminUserManagementApplicationService, AdminUserManagementApplicationService>();
        services.AddScoped<IClientUserManagementApplicationService, ClientUserManagementApplicationService>();
        services.AddScoped<IClientAccountAdminDeletionService, ClientAccountAdminDeletionService>();
        services.AddScoped<IDrugInteractionToolReportApplicationService, DrugInteractionToolReportApplicationService>();
        services.AddScoped<IDrugToolDetailApplicationService, DrugToolDetailApplicationService>();

        return services;
    }
}

