namespace DrugsApp.Domain.RegularDrugsFeature.Enums;

/// <summary>
/// Reason a regular drug schedule entered history. [description]
/// </summary>
public enum RegularDrugHistoryReason
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
