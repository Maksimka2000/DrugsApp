namespace DrugsApp.Host.Hangfire.RegularSupplementsFeature;

/// <summary>
/// Settings for the regular supplement expiration Hangfire recurring job.
/// </summary>
public sealed class RegularSupplementExpirationJobSettings
{
    /// <summary>
    /// Configuration section path for regular supplement expiration job.
    /// </summary>
    public const string SectionName = "RegularSupplements:ExpirationJob";

    /// <summary>
    /// Whether the job is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Cron expression controlling recurrence.
    /// </summary>
    public string CronExpression { get; init; } = "59 2 * * *";
}
