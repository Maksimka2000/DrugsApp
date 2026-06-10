namespace DrugsApp.Host.Hangfire.RegularSupplementsFeature;

/// <summary>
/// Settings for the regular supplement expiration Hangfire recurring job.
/// </summary>
public sealed class RegularSupplementExpirationJobSettings
{
    public const string SectionName = "RegularSupplements:ExpirationJob";

    public bool Enabled { get; init; } = true;

    public string CronExpression { get; init; } = "59 2 * * *";
}
