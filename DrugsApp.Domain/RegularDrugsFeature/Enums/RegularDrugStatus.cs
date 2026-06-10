namespace DrugsApp.Domain.RegularDrugsFeature.Enums;

/// <summary>
/// Lifecycle state for a user-owned regular drug intake schedule. [description]
/// </summary>
public enum RegularDrugStatus
{
    /// <summary>
    /// Schedule has a future start date and is not yet active.
    /// </summary>
    Scheduled,

    /// <summary>
    /// Schedule is currently active on the live list.
    /// </summary>
    Active,

    /// <summary>
    /// Schedule was archived to history.
    /// </summary>
    Expired
}
