namespace DrugsApp.Host.Hangfire.RegularDrugsFeature;

/// <summary>
/// Settings for the regular drug expiration Hangfire recurring job.
/// </summary>
public sealed class RegularDrugExpirationJobSettings
{
    /// <summary>
    /// Configuration section path for regular drug expiration job.
    /// </summary>
    public const string SectionName = "RegularDrugs:ExpirationJob";

    /// <summary>
    /// Whether the job is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Cron expression controlling recurrence.
    /// </summary>
    public string CronExpression { get; init; } = "59 2 * * *";
}
