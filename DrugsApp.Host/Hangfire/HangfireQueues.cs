namespace DrugsApp.Host.Hangfire;

/// <summary>
/// Hangfire queue names used to isolate notification and import workloads.
/// </summary>
public static class HangfireQueues
{
    /// <summary>
    /// Notification projection and reminder jobs.
    /// </summary>
    public const string Notifications = "notifications";

    /// <summary>
    /// Regular intake schedule expiration jobs.
    /// </summary>
    public const string RegularIntakes = "regular-intakes";

    /// <summary>
    /// Temp-drug import and revert jobs.
    /// </summary>
    public const string Imports = "imports";

    /// <summary>
    /// Fallback queue for jobs without an explicit queue.
    /// </summary>
    public const string Default = "default";
}
