namespace DrugsApp.Host.Hangfire.RegularFoodFeature;

/// <summary>
/// Settings for the regular food expiration Hangfire recurring job.
/// </summary>
public sealed class RegularFoodExpirationJobSettings
{
    public const string SectionName = "RegularFood:ExpirationJob";

    public bool Enabled { get; init; } = true;

    public string CronExpression { get; init; } = "59 2 * * *";
}
