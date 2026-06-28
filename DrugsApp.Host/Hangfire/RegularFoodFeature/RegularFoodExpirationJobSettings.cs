namespace DrugsApp.Host.Hangfire.RegularFoodFeature;

/// <summary>
/// Settings for the regular food expiration Hangfire recurring job.
/// </summary>
public sealed class RegularFoodExpirationJobSettings
{
    /// <summary>
    /// Configuration section path for regular food expiration job.
    /// </summary>
    public const string SectionName = "RegularFood:ExpirationJob";

    /// <summary>
    /// Whether the job is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Cron expression controlling recurrence.
    /// </summary>
    public string CronExpression { get; init; } = "59 2 * * *";
}
