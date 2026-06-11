namespace DrugsApp.Features.DrugToolFeature.Shared;

/// <summary>
/// Client interaction filter for drug/food/supplement sections [description].
/// </summary>
public enum DrugToolInteractionScope
{
    /// <summary>
    /// Returns userdata and catalog subsections.
    /// </summary>
    All,

    /// <summary>
    /// Returns only the userdata subsection.
    /// </summary>
    UserOnly
}

/// <summary>
/// Subsection discriminator inside drug/food/supplement interaction sections [description].
/// </summary>
public enum DrugToolInteractionSubsectionCode
{
    /// <summary>
    /// Interactions intersecting the user's live regular intake data.
    /// </summary>
    UserData,

    /// <summary>
    /// All catalog interactions for the selected drug (food/supplement sections).
    /// </summary>
    All,

    /// <summary>
    /// All catalog drug interactions (drug section).
    /// </summary>
    AllOther
}
