namespace DrugsApp.Domain.RegularFoodFeature.Enums;

/// <summary>
/// Persisted lifecycle state for user-owned regular food schedules.
/// </summary>
public enum RegularFoodStatus
{
    /// <summary>
    /// Schedule start date is in the future.
    /// </summary>
    Scheduled,

    /// <summary>
    /// Schedule is currently active for the user.
    /// </summary>
    Active,

    /// <summary>
    /// Schedule was archived to history.
    /// </summary>
    Expired,
}
