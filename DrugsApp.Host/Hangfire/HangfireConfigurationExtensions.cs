using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.Host.Hangfire;

/// <summary>
/// Registers Hangfire services and middleware for the host project.
/// </summary>
public static class HangfireConfigurationExtensions
{
    /// <summary>
    /// Configures Hangfire storage, worker server, and retry policy.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration source.</param>
    /// <returns>Updated service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Postgres connection string is missing.</exception>
    public static IServiceCollection AddHangfireProcessing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Read settings from appsettings
        HangfireSettings settings = configuration.GetSection(HangfireSettings.SectionName).Get<HangfireSettings>() ?? new();
        services.AddSingleton(settings);

        // Configure Hangfire storage; the connection string is resolved from the final configuration at first use
        services.AddHangfire((serviceProvider, hangfireConfiguration) =>
        {
            string? postgresConnectionString = serviceProvider
                .GetRequiredService<IConfiguration>()
                .GetConnectionString("Postgres");
            if (string.IsNullOrWhiteSpace(postgresConnectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:Postgres must be configured for Hangfire.");
            }

            hangfireConfiguration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(
                    options => options.UseNpgsqlConnection(postgresConnectionString),
                    new PostgreSqlStorageOptions
                    {
                        QueuePollInterval = TimeSpan.FromSeconds(settings.QueuePollIntervalSeconds)
                    });
        });

        // Configure automatic retry policy
        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
        {
            Attempts = settings.RetryAttempts,
            OnAttemptsExceeded = AttemptsExceededAction.Fail
        });

        // Configure Hangfire worker server
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = settings.WorkerCount;
            options.SchedulePollingInterval = TimeSpan.FromSeconds(settings.SchedulePollingIntervalSeconds);
            options.Queues = settings.Queues;
        });

        return services;
    }

    /// <summary>
    /// Enables Hangfire dashboard when configured.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <returns>Application builder.</returns>
    public static IApplicationBuilder UseConfiguredHangfireDashboard(this IApplicationBuilder app)
    {
        HangfireSettings settings = app.ApplicationServices.GetRequiredService<HangfireSettings>();
        if (!settings.EnableDashboard)
        {
            return app;
        }

        app.UseHangfireDashboard(
            settings.DashboardPath,
            new DashboardOptions
            {
                Authorization = new[] { new AdminDashboardAuthorizationFilter() }
            });
        return app;
    }
}
