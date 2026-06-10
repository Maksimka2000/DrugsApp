namespace DrugsApp.Host.Hangfire.RegularDrugsFeature;

/// <summary>
/// Settings for the regular drug expiration Hangfire recurring job.
/// </summary>
public sealed class RegularDrugExpirationJobSettings
{
    public const string SectionName = "RegularDrugs:ExpirationJob";

    public bool Enabled { get; init; } = true;

    public string CronExpression { get; init; } = "59 2 * * *";
}
