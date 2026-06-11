namespace DrugsApp.Features.DrugToolFeature.Shared;

/// <summary>
/// Client interaction filter for drug/food/supplement sections [description].
/// </summary>
public enum DrugToolInteractionScope
{
    /// <summary>
    /// Returns user matches and catalog remainder subsections.
    /// </summary>
    All,

    /// <summary>
    /// Returns only targets intersecting the user's live regular intake data.
    /// </summary>
    UserOnly
}

/// <summary>
/// Top-level accordion section discriminator in the drug tool response [description].
/// </summary>
public enum DrugToolInteractionSectionCode
{
    /// <summary>
    /// Alcohol interaction accordion section.
    /// </summary>
    Alcohol,

    /// <summary>
    /// Drug-drug interaction accordion section.
    /// </summary>
    Drug,

    /// <summary>
    /// Food interaction accordion section.
    /// </summary>
    Food,

    /// <summary>
    /// Supplement interaction accordion section.
    /// </summary>
    Supplement
}

/// <summary>
/// Optional split inside drug/food/supplement interaction sections [description].
/// </summary>
public enum DrugToolInteractionSubsectionCode
{
    /// <summary>
    /// Targets intersecting the user's live regular intake data.
    /// </summary>
    UserMatches,

    /// <summary>
    /// Remaining interacted catalog targets not matched to the user.
    /// </summary>
    CatalogRemainder
}
