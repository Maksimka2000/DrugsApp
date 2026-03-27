namespace DrugsApp.Host.Hangfire;

/// <summary>
/// Strongly typed settings for Hangfire host-level configuration.
/// </summary>
public sealed class HangfireSettings
{
    /// <summary>
    /// name
    /// </summary>
    public const string SectionName = "Hangfire";

    /// <summary>
    /// Dashboard route segment (for example: /hangfire).
    /// </summary>
    public string DashboardPath { get; init; } = "/hangfire";

    /// <summary>
    /// Enables or disables Hangfire dashboard middleware.
    /// </summary>
    public bool EnableDashboard { get; init; } = true;

    /// <summary>
    /// Number of Hangfire workers in this host process.
    /// </summary>
    public int WorkerCount { get; init; } = 1;

    /// <summary>
    /// Polling interval for scheduled jobs.
    /// </summary>
    public int SchedulePollingIntervalSeconds { get; init; } = 3600;

    /// <summary>
    /// Queue poll interval used by PostgreSQL storage.
    /// </summary>
    public int QueuePollIntervalSeconds { get; init; } = 180;

    /// <summary>
    /// Number of automatic retries before marking a job as failed.
    /// </summary>
    public int RetryAttempts { get; init; } = 3;

    /// <summary>
    /// Queue names consumed by this worker.
    /// </summary>
    public string[] Queues { get; init; } = ["default"];
}
