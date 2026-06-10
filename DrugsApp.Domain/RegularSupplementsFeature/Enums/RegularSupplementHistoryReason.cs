namespace DrugsApp.Domain.RegularSupplementsFeature.Enums;

/// <summary>
/// Reason a regular supplement schedule entered history. [description]
/// </summary>
public enum RegularSupplementHistoryReason
{
    /// <summary>
    /// Schedule end date passed and the background job archived the row.
    /// </summary>
    AutoExpire,

    /// <summary>
    /// User manually moved the schedule to history.
    /// </summary>
    ManualRemove
}
