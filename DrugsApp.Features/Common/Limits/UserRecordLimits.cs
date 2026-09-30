using ErrorOr;

namespace DrugsApp.Features.Common.Limits;

/// <summary>
/// Per-user record caps enforced whenever profile records are created, so no single account grows unbounded [description].
/// </summary>
internal static class UserRecordLimits
{
    /// <summary>
    /// Maximum records of one profile type per user.
    /// </summary>
    /// <remarks>
    /// Applies separately to drug, supplement, food and environmental allergies, health conditions and drug problems; catalog and custom entries count together.
    /// </remarks>
    internal const int MAX_PROFILE_RECORDS_PER_TYPE = 50;

    /// <summary>
    /// Maximum live regular drugs per user.
    /// </summary>
    /// <remarks>
    /// History rows are not counted.
    /// </remarks>
    internal const int MAX_REGULAR_DRUGS = 40;

    /// <summary>
    /// Maximum live regular supplements per user.
    /// </summary>
    /// <remarks>
    /// History rows are not counted.
    /// </remarks>
    internal const int MAX_REGULAR_SUPPLEMENTS = 60;

    /// <summary>
    /// Maximum live regular foods per user.
    /// </summary>
    /// <remarks>
    /// History rows are not counted.
    /// </remarks>
    internal const int MAX_REGULAR_FOODS = 100;

    /// <summary>
    /// Maximum saved meal times (the diet schedule) per user.
    /// </summary>
    /// <remarks>
    /// Meal times drive oral-intake guidance, so the cap keeps that lookup small.
    /// </remarks>
    internal const int MAX_MEAL_TIMES = 20;

    /// <summary>
    /// Fails with a conflict when the user already holds the maximum number of records.
    /// </summary>
    /// <param name="currentCount">Records the user already owns.</param>
    /// <param name="maxCount">Maximum allowed for this record type.</param>
    /// <param name="resourceType">Record type name used in the error code and metadata.</param>
    /// <returns>Success while below the cap, otherwise a conflict error carrying the cap.</returns>
    internal static ErrorOr<Success> EnsureBelowLimit(int currentCount, int maxCount, string resourceType)
    {
        if (currentCount < maxCount)
        {
            return Result.Success;
        }

        return Error.Conflict(
            code: $"{resourceType}.LimitReached",
            description: $"{resourceType} limit reached.",
            metadata: new()
            {
                ["field"] = "limit",
                ["resourceType"] = resourceType,
                ["maxCount"] = maxCount,
            });
    }
}
