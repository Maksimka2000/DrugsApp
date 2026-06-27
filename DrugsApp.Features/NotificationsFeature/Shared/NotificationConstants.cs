using System.Collections.Generic;

namespace DrugsApp.Features.NotificationsFeature.Shared;

/// <summary>
/// Static catalog values reused by notification hydration.
/// </summary>
public static class NotificationConstants
{
    /// <summary>
    /// Default weekly health review areas used in reminder notifications.
    /// </summary>
    public static readonly IReadOnlyList<string> WeeklyHealthReviewAreas = new[]
    {
        "Allergies",
        "Drug problems",
        "Health conditions",
        "Regular drugs",
        "Regular food",
        "Regular supplements"
    };
}
