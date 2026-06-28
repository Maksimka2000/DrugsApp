namespace DrugsApp.Host.Hangfire.NotificationsFeature;

/// <summary>
/// Settings for the weekly health-data reminder Hangfire recurring job.
/// </summary>
public sealed class WeeklyHealthDataReminderJobSettings
{
    /// <summary>
    /// Configuration section path for weekly health reminder job.
    /// </summary>
    public const string SectionName = "Notifications:WeeklyHealthDataReminder";

    /// <summary>
    /// Whether the reminder job is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Cron expression controlling recurrence.
    /// </summary>
    public string CronExpression { get; init; } = "0 8 * * 1";
}
