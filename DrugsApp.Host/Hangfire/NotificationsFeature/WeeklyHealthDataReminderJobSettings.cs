namespace DrugsApp.Host.Hangfire.NotificationsFeature;

/// <summary>
/// Settings for the weekly health-data reminder Hangfire recurring job.
/// </summary>
public sealed class WeeklyHealthDataReminderJobSettings
{
    public const string SectionName = "Notifications:WeeklyHealthDataReminder";

    public bool Enabled { get; init; } = true;

    public string CronExpression { get; init; } = "0 8 * * 1";
}
