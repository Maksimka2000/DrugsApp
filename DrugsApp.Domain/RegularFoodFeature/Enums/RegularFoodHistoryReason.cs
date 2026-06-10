namespace DrugsApp.Domain.RegularFoodFeature.Enums;

/// <summary>
/// Reason a regular food schedule entered history.
/// </summary>
public enum RegularFoodHistoryReason
{
    /// <summary>
    /// Row was archived automatically after the end date passed.
    /// </summary>
    AutoExpire,

    /// <summary>
    /// Row was archived manually by the user.
    /// </summary>
    ManualRemove,
}
