using System;
using System.Net;
using System.Net.Mail;
using Azure.Communication.Email;
using Azure.Storage.Blobs;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Domain.AccountFeature.Interfaces;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.TestFeature.Repositories;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Domain.VetDrugFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Domain.SupportFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Domain.SystemTextFeature.Repositories;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.NotificationsFeature.Repositories;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Infrastructure.Repositories.PatientSupervisionFeature;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.ExternalServices.BlobStorage;
using DrugsApp.Infrastructure.ExternalServices.Email;
using DrugsApp.Infrastructure.Repositories.ActiveSubstanceFeature;
using DrugsApp.Infrastructure.Repositories.ActiveSubstanceGroupFeature;
using DrugsApp.Infrastructure.Repositories.AccountFeature;
using DrugsApp.Infrastructure.Repositories.AuthFeature;
using DrugsApp.Infrastructure.Repositories.DrugFeature;
using DrugsApp.Infrastructure.Repositories.FoodFeature;
using DrugsApp.Infrastructure.Repositories.PharmaceuticalFormFeature;
using DrugsApp.Infrastructure.Repositories.RouteOfAdministrationFeature;
using DrugsApp.Infrastructure.Repositories.InteractionFeature;
using DrugsApp.Infrastructure.Repositories.TestFeature;
using DrugsApp.Infrastructure.Repositories.TempDrugFeature;
using DrugsApp.Infrastructure.Repositories.VetDrugFeature;
using DrugsApp.Infrastructure.Repositories.SupplementFeature;
using DrugsApp.Infrastructure.Repositories.SupportFeature;
using DrugsApp.Infrastructure.Repositories.SystemTextFeature;
using DrugsApp.Infrastructure.Repositories.UserProfileFeature;
using DrugsApp.Infrastructure.Repositories.HealthConditionFeature;
using DrugsApp.Infrastructure.Repositories.AllergiesFeature;
using DrugsApp.Infrastructure.Repositories.DrugProblemsFeature;
using DrugsApp.Infrastructure.Repositories.MealTimeManagementFeature;
using DrugsApp.Infrastructure.Repositories.NotificationsFeature;
using DrugsApp.Infrastructure.Repositories.RegularDrugsFeature;
using DrugsApp.Infrastructure.Repositories.RegularSupplementsFeature;
using DrugsApp.Infrastructure.Repositories.RegularFoodFeature;
using DrugsApp.Infrastructure.Services.NotificationsFeature;
using DrugsApp.Infrastructure.Persistence.Identity;
using DrugsApp.Infrastructure.Services.AuthFeature;
using DrugsApp.Infrastructure.Services.Seed;
using Microsoft.AspNetCore.Identity;
using Npgsql;
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
        // Register DbContext (NpgsqlDataSource enables jsonb deserialization for catalog aliases)
        string postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddSingleton<NpgsqlDataSource>(_ =>
            new NpgsqlDataSourceBuilder(postgresConnectionString)
                .EnableDynamicJson()
                .Build());

        services.AddDbContext<DrugsAppDbContext>((serviceProvider, options) =>
            options.UseNpgsql(serviceProvider.GetRequiredService<NpgsqlDataSource>()));

        // regis cache registration
        string? redisConnection = configuration.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);

        // 
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
        })
            .AddRoles<ApplicationRole>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserStore<ApplicationUser>, ApplicationUserStore>();
        services.AddScoped<IRoleStore<ApplicationRole>, ApplicationRoleStore>();

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
        services.AddScoped<IFaqEntryRepository, FaqEntryRepository>();
        services.AddScoped<ISystemTextRepository, SystemTextRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IClientReproductiveHealthProfileRepository, ClientReproductiveHealthProfileRepository>();
        services.AddScoped<IHealthcareProviderProfileRepository, HealthcareProviderProfileRepository>();
        services.AddScoped<IPendingEmailChangeRepository, PendingEmailChangeRepository>();
        services.AddScoped<IHealthConditionRepository, HealthConditionRepository>();
        services.AddScoped<IUserHealthConditionRepository, UserHealthConditionRepository>();
        services.AddScoped<IDrugProblemRepository, DrugProblemRepository>();
        services.AddScoped<IDrugProblemTypeRepository, DrugProblemTypeRepository>();
        services.AddScoped<IClientAllergySummaryRepository, ClientAllergySummaryRepository>();
        services.AddScoped<IClientDrugAllergyRepository, ClientDrugAllergyRepository>();
        services.AddScoped<IClientSupplementAllergyRepository, ClientSupplementAllergyRepository>();
        services.AddScoped<IClientFoodAllergyRepository, ClientFoodAllergyRepository>();
        services.AddScoped<IClientOtherAllergyRepository, ClientOtherAllergyRepository>();
        services.AddScoped<IClientMealTimeRepository, ClientMealTimeRepository>();
        services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
        services.AddScoped<ISystemAnnouncementRepository, SystemAnnouncementRepository>();
        services.AddScoped<IClientRegularDrugRepository, ClientRegularDrugRepository>();
        services.AddScoped<IClientRegularSupplementRepository, ClientRegularSupplementRepository>();
        services.AddScoped<IClientOwnSupplementRepository, ClientOwnSupplementRepository>();
        services.AddScoped<IClientRegularFoodRepository, ClientRegularFoodRepository>();
        services.AddScoped<IUserSecurityStatusReadService, UserSecurityStatusReadService>();
        services.AddScoped<IPatientSupervisionNotificationReadService, PatientSupervisionNotificationReadService>();

        // Patient Supervision
        services.AddScoped<ISupervisionRequestRepository, SupervisionRequestRepository>();
        services.AddScoped<ISupervisionConnectionRepository, SupervisionConnectionRepository>();
        services.AddScoped<IMedicalChangeProposalRepository, MedicalChangeProposalRepository>();
        services.AddScoped<ISupervisionReadRepository, SupervisionReadRepository>();
        services.AddScoped<IUserRoleReadService, UserRoleReadService>();


        // Register Email External services here
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        EmailSettings emailSettings = configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>() ?? new();

        if (emailSettings.UsesAzureCommunicationServices)
        {
            if (string.IsNullOrWhiteSpace(emailSettings.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Email:ConnectionString must be configured when Email:Provider is AzureCommunicationServices.");
            }

            services.AddSingleton(_ => new EmailClient(emailSettings.ConnectionString));
            services.AddScoped<IEmailVerificationSender, AzureCommunicationEmailVerificationSender>();
        }
        else
        {
            services
                .AddFluentEmail(emailSettings.SenderEmail, emailSettings.SenderName)
                .AddSmtpSender(() =>
                {
                    var client = new SmtpClient(emailSettings.SmtpHost, emailSettings.SmtpPort);

                    if (!string.IsNullOrWhiteSpace(emailSettings.SmtpUsername) &&
                        !string.IsNullOrWhiteSpace(emailSettings.SmtpPassword))
                    {
                        client.Credentials = new NetworkCredential(
                            emailSettings.SmtpUsername,
                            emailSettings.SmtpPassword);
                        client.EnableSsl = true;
                    }

                    return client;
                });

            services.AddScoped<IEmailVerificationSender, EmailVerificationSender>();
        }
        services.Configure<SuperAdminSettings>(configuration.GetSection(SuperAdminSettings.SectionName));
        services.AddScoped<IIdentityAuthenticationService, IdentityAuthenticationService>();
        services.AddScoped<IIdentityUserManagementService, IdentityUserManagementService>();
        services.AddScoped<IIdentityUserQueryRepository, IdentityUserQueryRepository>();
        services.AddScoped<ISuperAdminGuard, SuperAdminGuard>();
        services.AddScoped<IClientAccountTeardownRepository, ClientAccountTeardownRepository>();
        services.AddScoped<IUserPermissionService, UserPermissionServiceDb>();
        services.Decorate<IUserPermissionService, UserPermissionServiceCached>();
        services.AddScoped<IdentityAuthorizationDataSeeder>();
        services.Configure<DevSeedSettings>(configuration.GetSection(DevSeedSettings.SectionName));
        services.AddScoped<PatientDevDataSeeder>();
        services.AddHttpClient("ImportDownload", client => client.Timeout = TimeSpan.FromMinutes(5));

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
