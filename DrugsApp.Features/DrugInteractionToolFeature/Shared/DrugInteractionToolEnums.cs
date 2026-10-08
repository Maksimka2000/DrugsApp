using System.Text.Json.Serialization;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Internal audience discriminator for shared report orchestration [description].
/// </summary>
internal enum ReportAudience
{
    /// <summary>
    /// Anonymous public tool (CASE TWO).
    /// </summary>
    Public = 0,

    /// <summary>
    /// Authenticated client tool (CASE ONE).
    /// </summary>
    Registered = 1
}

/// <summary>
/// Origin of a drug row in the registered report [description].
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SelectedDrugSource
{
    /// <summary>
    /// Manually selected form id from the request.
    /// </summary>
    [JsonStringEnumMemberName("manual")]
    Manual = 0,

    /// <summary>
    /// Merged from the user's live regular drug schedule.
    /// </summary>
    [JsonStringEnumMemberName("user_regular")]
    UserRegular = 1,

    /// <summary>
    /// Present both manually and as a regular drug.
    /// </summary>
    [JsonStringEnumMemberName("manual_and_user_regular")]
    ManualAndUserRegular = 2
}

/// <summary>
/// Role a drug plays in a duplication warning [description].
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DuplicationDrugSource
{
    /// <summary>
    /// The drug is on the user's live regular intake list.
    /// </summary>
    [JsonStringEnumMemberName("takingNow")]
    TakingNow = 0,

    /// <summary>
    /// The drug was selected for this report.
    /// </summary>
    [JsonStringEnumMemberName("selected")]
    Selected = 1
}

/// <summary>
/// Duplication warning classification [description].
/// </summary>
public enum DuplicationKind
{
    /// <summary>
    /// Two or more drugs share the same active substance.
    /// </summary>
    ExactActiveSubstance = 0,

    /// <summary>
    /// Two or more drugs share membership in an active substance group.
    /// </summary>
    ActiveSubstanceGroup = 1,

    /// <summary>
    /// Two or more forms of the same drug are selected or taken together.
    /// </summary>
    Drug = 2,

    /// <summary>
    /// A selected drug form is already on the user's live regular intake list.
    /// </summary>
    DrugForm = 3
}

/// <summary>
/// Allergy record origin for report rows [description].
/// </summary>
public enum AllergyRecordSourceDto
{
    /// <summary>
    /// Catalog-backed allergy entry.
    /// </summary>
    Database = 0,

    /// <summary>
    /// User-entered custom allergy entry.
    /// </summary>
    Custom = 1
}

/// <summary>
/// Food or supplement party kind on interaction rows [description].
/// </summary>
public enum InteractionPartyKind
{
    /// <summary>
    /// Single catalog food item.
    /// </summary>
    Food = 0,

    /// <summary>
    /// Food group aggregate.
    /// </summary>
    FoodGroup = 1,

    /// <summary>
    /// Single catalog supplement item.
    /// </summary>
    Supplement = 2,

    /// <summary>
    /// Supplement group aggregate.
    /// </summary>
    SupplementGroup = 3
}
