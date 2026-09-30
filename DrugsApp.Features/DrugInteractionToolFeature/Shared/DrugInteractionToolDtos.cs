using System.Text.Json.Serialization;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Generic foldable subsection wrapper for report UI [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="HeadlineText">Headline displayed for the subsection.</param>
/// <param name="RecordCount">Number of rows contained.</param>
/// <param name="Rows">Rows belonging to the subsection.</param>
public sealed record ReportSubsectionReadModel<TRow>(
    string SectionCode,
    string HeadlineText,
    int RecordCount,
    IReadOnlyCollection<TRow> Rows);

/// <summary>
/// Slim drug reference used in selected-drug lists, comparisons, and duplication sections [description].
/// </summary>
/// <param name="DrugId">Identifier of the drug.</param>
/// <param name="DrugFormId">Identifier of the drug form.</param>
/// <param name="DrugSummaryText">Readable summary of the drug.</param>
/// <param name="IsOral">Indicates whether the form is oral.</param>
public sealed record DrugTargetSummaryReadModel(
    Guid DrugId,
    Guid DrugFormId,
    string DrugSummaryText,
    bool IsOral);

/// <summary>
/// Selected form summary embedded in per-drug report headers [description].
/// </summary>
/// <param name="FormulationName">Pharmaceutical formulation name.</param>
/// <param name="Dosage">Dosage string.</param>
/// <param name="IsOral">Indicates whether the form is oral.</param>
/// <param name="RoutesOfAdministration">Administration routes list.</param>
/// <param name="PharmaceuticalForms">Pharmaceutical forms list.</param>
/// <param name="Leaflet">Leaflet reference.</param>
/// <param name="EducationMaterials">Education materials reference.</param>
public sealed record SelectedDrugFormSummaryReadModel(
    string FormulationName,
    string Dosage,
    bool IsOral,
    IReadOnlyCollection<string> RoutesOfAdministration,
    IReadOnlyCollection<string> PharmaceuticalForms,
    string? Leaflet,
    string? EducationMaterials);

/// <summary>
/// Active substance reference embedded in per-drug report headers [description].
/// </summary>
/// <param name="Id">Identifier of the active substance.</param>
/// <param name="Name">Display name of the active substance.</param>
public sealed record ActiveSubstanceRefReadModel(
    Guid Id,
    string Name);

/// <summary>
/// Duplication-friendly drug summary without form id [description].
/// </summary>
/// <param name="DrugId">Identifier of the drug.</param>
/// <param name="OnlyDrugSummaryText">Summary without form.</param>
/// <param name="IsOral">Indicates whether the form is oral.</param>
public sealed record DuplicationDrugSummaryReadModel(
    Guid DrugId,
    string OnlyDrugSummaryText,
    bool IsOral);

/// <summary>
/// Regular-medication schedule attached when a specific-drug row comes from the user's live intake [description].
/// </summary>
/// <param name="IntakeTimes">Daily clock times in HH:mm.</param>
/// <param name="StartDate">ISO date when the course starts, or null when ongoing.</param>
/// <param name="EndDate">ISO date when the course ends, or null when open-ended.</param>
/// <param name="IsOngoing">True when both dates are absent.</param>
public sealed record RegularDrugIntakeReadModel(
    IReadOnlyCollection<string> IntakeTimes,
    string? StartDate,
    string? EndDate,
    bool IsOngoing);

/// <summary>
/// Expanded per-drug header for specific drug report blocks [description].
/// </summary>
/// <param name="DrugId">Identifier of the drug.</param>
/// <param name="CommercialName">Commercial name of the drug.</param>
/// <param name="TypeOfProduct">Type of product.</param>
/// <param name="TypeOfDrug">Drug type enumeration value.</param>
/// <param name="SelectedDrugFormId">Identifier of the selected form.</param>
/// <param name="SelectedDrugFormSummary">Summary of the selected form.</param>
/// <param name="ActiveSubstances">Active substances included.</param>
/// <param name="SelectedDrugSource">How the form entered the report; null on public reports.</param>
/// <param name="RegularIntake">Schedule copied from the matching live regular-drug row.</param>
public sealed record SpecificDrugDetailReadModel(
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    Guid SelectedDrugFormId,
    SelectedDrugFormSummaryReadModel SelectedDrugFormSummary,
    IReadOnlyCollection<ActiveSubstanceRefReadModel> ActiveSubstances,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    SelectedDrugSource? SelectedDrugSource = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    RegularDrugIntakeReadModel? RegularIntake = null);

/// <summary>
/// Public tool settings snapshot (CASE TWO) [description].
/// </summary>
/// <param name="ShowAllDrugsInteractions">Flag to include all drug interactions.</param>
/// <param name="ShowOralGuidance">Flag to include oral guidance.</param>
/// <param name="ShowAllFoodAndSupplementInteractions">Flag to include all food/supplement interactions.</param>
/// <param name="ManualDrugLimit">Maximum manual drug selection count.</param>
public sealed record PublicDrugInteractionToolSettingsSnapshotReadModel(
    bool ShowAllDrugsInteractions,
    bool ShowOralGuidance,
    bool ShowAllFoodAndSupplementInteractions,
    int ManualDrugLimit);

/// <summary>
/// Registered tool settings snapshot (CASE ONE) [description].
/// </summary>
/// <param name="IncludeUserData">Flag to include user data.</param>
/// <param name="ShowAllDrugsInteractions">Flag to include all drug interactions.</param>
/// <param name="ShowOralGuidance">Flag to include oral guidance.</param>
/// <param name="ShowAllFoodAndSupplementInteractions">Flag to include all food/supplement interactions.</param>
/// <param name="ManualDrugLimit">Maximum manual drug selection count.</param>
public sealed record RegisteredDrugInteractionToolSettingsSnapshotReadModel(
    bool IncludeUserData,
    bool ShowAllDrugsInteractions,
    bool ShowOralGuidance,
    bool ShowAllFoodAndSupplementInteractions,
    int ManualDrugLimit);

/// <summary>
/// Substance pair interaction row reused across per-drug and global sections [description].
/// </summary>
/// <param name="CurrentActiveSubstanceId">Current active substance identifier.</param>
/// <param name="CurrentActiveSubstanceName">Current active substance name.</param>
/// <param name="ComparedActiveSubstanceId">Compared active substance identifier.</param>
/// <param name="ComparedActiveSubstanceName">Compared active substance name.</param>
/// <param name="InteractionId">Interaction identifier.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="StomachText">Optional stomach-related text.</param>
/// <param name="Severity">Interaction severity label.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record SubstancePairInteractionRowReadModel(
    Guid CurrentActiveSubstanceId,
    string CurrentActiveSubstanceName,
    Guid ComparedActiveSubstanceId,
    string ComparedActiveSubstanceName,
    Guid InteractionId,
    string DefaultText,
    string? StomachText,
    string Severity,
    string DisplayLine);

/// <summary>
/// Cross-drug interaction group for a single compared drug [description].
/// </summary>
/// <param name="GroupTitle">Title of the compared drug group.</param>
/// <param name="CurrentDrug">Current drug summary.</param>
/// <param name="ComparedDrug">Compared drug summary.</param>
/// <param name="RecordCount">Number of interaction rows.</param>
/// <param name="PairRows">Interaction rows for the pair.</param>
public sealed record ComparedDrugInteractionGroupReadModel(
    string GroupTitle,
    DrugTargetSummaryReadModel CurrentDrug,
    DrugTargetSummaryReadModel ComparedDrug,
    int RecordCount,
    IReadOnlyCollection<SubstancePairInteractionRowReadModel> PairRows);

/// <summary>
/// Per-drug cross-drug interaction section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="RecordCount">Number of compared drug groups.</param>
/// <param name="ComparedDrugGroups">Groups of compared drugs.</param>
public sealed record DrugInteractionSectionReadModel(
    string SectionCode,
    int RecordCount,
    IReadOnlyCollection<ComparedDrugInteractionGroupReadModel> ComparedDrugGroups);

/// <summary>
/// Alcohol interaction row for a single active substance [description].
/// </summary>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="SpecialText">Optional special text.</param>
public sealed record AlcoholInteractionRowReadModel(
    string ActiveSubstanceName,
    string DefaultText,
    string? SpecialText);

/// <summary>
/// Per-drug alcohol interaction section [description].
/// </summary>
/// <param name="HeadlineText">Headline text of the section.</param>
/// <param name="Rows">Alcohol interaction rows.</param>
public sealed record AlcoholInteractionSectionReadModel(
    string HeadlineText,
    IReadOnlyCollection<AlcoholInteractionRowReadModel> Rows);

/// <summary>
/// Food interaction row [description].
/// </summary>
/// <param name="ActiveSubstanceId">Active substance identifier.</param>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="FoodId">Food identifier.</param>
/// <param name="FoodName">Food name.</param>
/// <param name="InteractionId">Interaction identifier.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="StomachText">Optional stomach-related text.</param>
/// <param name="Severity">Interaction severity.</param>
/// <param name="IsTakenNow">Whether the food is taken now.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record FoodInteractionRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    Guid FoodId,
    string FoodName,
    Guid InteractionId,
    string DefaultText,
    string? StomachText,
    string Severity,
    bool IsTakenNow,
    string DisplayLine);

/// <summary>
/// Supplement interaction row [description].
/// </summary>
/// <param name="ActiveSubstanceId">Active substance identifier.</param>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="SupplementId">Supplement identifier.</param>
/// <param name="SupplementName">Supplement name.</param>
/// <param name="InteractionId">Interaction identifier.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="StomachText">Optional stomach-related text.</param>
/// <param name="Severity">Interaction severity.</param>
/// <param name="IsTakenNow">Whether the supplement is taken now.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record SupplementInteractionRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    Guid SupplementId,
    string SupplementName,
    Guid InteractionId,
    string DefaultText,
    string? StomachText,
    string Severity,
    bool IsTakenNow,
    string DisplayLine);

/// <summary>
/// Public per-drug food section (catalog allOther only) [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="AllOther">Subsection for all other interactions.</param>
public sealed record PublicFoodInteractionSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<FoodInteractionRowReadModel> AllOther);

/// <summary>
/// Public per-drug supplement section (catalog allOther only) [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="AllOther">Subsection for all other interactions.</param>
public sealed record PublicSupplementInteractionSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel> AllOther);

/// <summary>
/// Meal-time oral guidance subsection for registered reports [description].
/// </summary>
/// <param name="HeadlineText">Headline for the oral guidance block.</param>
/// <param name="MealIntakeSummary">Summary text for meal intake guidance.</param>
/// <param name="ActiveSubstanceGuidanceRows">Guidance rows per active substance.</param>
public sealed record OralGuidanceSubsectionReadModel(
    string HeadlineText,
    string MealIntakeSummary,
    IReadOnlyCollection<ActiveSubstanceOralGuidanceRowReadModel> ActiveSubstanceGuidanceRows);

/// <summary>
/// Per-substance oral guidance inside meal-time block [description].
/// </summary>
/// <param name="ActiveSubstanceId">Active substance identifier.</param>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="GuidanceText">Guidance text.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record ActiveSubstanceOralGuidanceRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    string GuidanceText,
    string DisplayLine);

/// <summary>
/// Registered per-drug food section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="OralGuidance">Optional oral guidance subsection.</param>
/// <param name="TakenNow">Optional taken-now subsection.</param>
/// <param name="AllOther">Optional all-other subsection.</param>
public sealed record RegisteredFoodInteractionSectionReadModel(
    string SectionCode,
    OralGuidanceSubsectionReadModel? OralGuidance,
    ReportSubsectionReadModel<FoodInteractionRowReadModel>? TakenNow,
    ReportSubsectionReadModel<FoodInteractionRowReadModel>? AllOther);

/// <summary>
/// Registered per-drug supplement section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="TakenNow">Optional taken-now subsection.</param>
/// <param name="AllOther">Optional all-other subsection.</param>
public sealed record RegisteredSupplementInteractionSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel>? TakenNow,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel>? AllOther);

/// <summary>
/// Per-drug problematic drug row [description].
/// </summary>
/// <param name="ProblemDescription">Description of the problem.</param>
/// <param name="HeadlineText">Headline text for the problem.</param>
public sealed record SpecificDrugProblemRowReadModel(
    string ProblemDescription,
    string HeadlineText);

/// <summary>
/// Per-drug problems section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="RecordCount">Number of problem rows.</param>
/// <param name="Rows">Problem rows.</param>
public sealed record SpecificDrugProblemsSectionReadModel(
    string SectionCode,
    int RecordCount,
    IReadOnlyCollection<SpecificDrugProblemRowReadModel> Rows);

/// <summary>
/// Direct drug allergy row on a specific drug [description].
/// </summary>
/// <param name="ProblemDescription">Description of the allergy problem.</param>
/// <param name="HeadlineText">Headline text for the allergy.</param>
public sealed record SpecificDrugDirectAllergyRowReadModel(
    string ProblemDescription,
    string HeadlineText);

/// <summary>
/// Active substance overlap allergy row on a specific drug [description].
/// </summary>
/// <param name="ActiveSubstanceId">Active substance identifier.</param>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="SourceAllergyDrugDisplay">Display text of the source allergy drug.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="HeadlineText">Headline text.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record ActiveSubstanceAllergyOverlapRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    string SourceAllergyDrugDisplay,
    string? UserNote,
    string HeadlineText,
    string DisplayLine);

/// <summary>
/// Per-drug allergies section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="RecordCount">Number of rows.</param>
/// <param name="DirectDrugAllergy">Direct drug allergy row if present.</param>
/// <param name="ActiveSubstanceOverlapRows">Active substance overlap rows.</param>
public sealed record SpecificDrugAllergiesSectionReadModel(
    string SectionCode,
    int RecordCount,
    SpecificDrugDirectAllergyRowReadModel? DirectDrugAllergy,
    IReadOnlyCollection<ActiveSubstanceAllergyOverlapRowReadModel> ActiveSubstanceOverlapRows);

/// <summary>
/// Public per-drug report block (CASE TWO) [description].
/// </summary>
/// <param name="Drug">Drug details.</param>
/// <param name="Alcohol">Alcohol section.</param>
/// <param name="Drugs">Drug interaction section.</param>
/// <param name="Foods">Optional food section.</param>
/// <param name="Supplements">Optional supplement section.</param>
public sealed record PublicSpecificDrugReportReadModel(
    SpecificDrugDetailReadModel Drug,
    AlcoholInteractionSectionReadModel Alcohol,
    DrugInteractionSectionReadModel Drugs,
    PublicFoodInteractionSectionReadModel? Foods,
    PublicSupplementInteractionSectionReadModel? Supplements);

/// <summary>
/// Registered per-drug report block (CASE ONE) [description].
/// </summary>
/// <param name="Drug">Drug details.</param>
/// <param name="Problems">Optional problems section.</param>
/// <param name="Allergies">Optional allergies section.</param>
/// <param name="Alcohol">Alcohol section.</param>
/// <param name="Drugs">Drug interaction section.</param>
/// <param name="Foods">Optional food section.</param>
/// <param name="Supplements">Optional supplement section.</param>
public sealed record RegisteredSpecificDrugReportReadModel(
    SpecificDrugDetailReadModel Drug,
    SpecificDrugProblemsSectionReadModel? Problems,
    SpecificDrugAllergiesSectionReadModel? Allergies,
    AlcoholInteractionSectionReadModel Alcohol,
    DrugInteractionSectionReadModel Drugs,
    RegisteredFoodInteractionSectionReadModel? Foods,
    RegisteredSupplementInteractionSectionReadModel? Supplements);

/// <summary>
/// Global drug pair group at report bottom [description].
/// </summary>
/// <param name="GroupTitle">Title of the group.</param>
/// <param name="LeftDrug">Left-side drug summary.</param>
/// <param name="RightDrug">Right-side drug summary.</param>
/// <param name="RecordCount">Number of pair rows.</param>
/// <param name="PairRows">Interaction rows.</param>
public sealed record GlobalDrugPairGroupReadModel(
    string GroupTitle,
    DrugTargetSummaryReadModel LeftDrug,
    DrugTargetSummaryReadModel RightDrug,
    int RecordCount,
    IReadOnlyCollection<SubstancePairInteractionRowReadModel> PairRows);

/// <summary>
/// Global all-drugs interaction matrix section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="RecordCount">Number of pair groups.</param>
/// <param name="PairGroups">Pair groups included.</param>
public sealed record AllDrugsInteractionsSectionReadModel(
    string SectionCode,
    int RecordCount,
    IReadOnlyCollection<GlobalDrugPairGroupReadModel> PairGroups);

/// <summary>
/// Exact active substance duplication row [description].
/// </summary>
/// <param name="ActiveSubstanceId">Active substance identifier.</param>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="AffectedDrugs">Drugs affected by duplication.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="DuplicationKind">Duplication classification.</param>
public sealed record ExactDuplicationRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    IReadOnlyCollection<DuplicationDrugSummaryReadModel> AffectedDrugs,
    string DisplayLine,
    DuplicationKind DuplicationKind);

/// <summary>
/// Active substance member of a duplication group [description].
/// </summary>
/// <param name="SubstanceId">Substance identifier.</param>
/// <param name="SubstanceName">Substance name.</param>
public sealed record GroupActiveSubstanceReadModel(
    Guid SubstanceId,
    string SubstanceName);

/// <summary>
/// Active substance from a group that appears in selected drugs [description].
/// </summary>
/// <param name="AffectedSubstanceId">Affected substance identifier.</param>
/// <param name="AffectedSubstanceName">Affected substance name.</param>
public sealed record AffectedActiveSubstanceReadModel(
    Guid AffectedSubstanceId,
    string AffectedSubstanceName);

/// <summary>
/// Active substance group duplication row [description].
/// </summary>
/// <param name="ActiveSubstanceGroupId">Active substance group identifier.</param>
/// <param name="GroupName">Group name.</param>
/// <param name="GroupActiveSubstances">Members of the group.</param>
/// <param name="AffectedActiveSubstances">Affected substances.</param>
/// <param name="AffectedDrugs">Affected drugs.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="DuplicationKind">Duplication classification.</param>
public sealed record GroupDuplicationRowReadModel(
    Guid ActiveSubstanceGroupId,
    string GroupName,
    IReadOnlyCollection<GroupActiveSubstanceReadModel> GroupActiveSubstances,
    IReadOnlyCollection<AffectedActiveSubstanceReadModel> AffectedActiveSubstances,
    IReadOnlyCollection<DuplicationDrugSummaryReadModel> AffectedDrugs,
    string DisplayLine,
    DuplicationKind DuplicationKind);

/// <summary>
/// Drug duplication warnings section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="ExactActiveSubstanceDuplicates">Exact duplication subsection.</param>
/// <param name="ActiveSubstanceGroupDuplicates">Group duplication subsection.</param>
public sealed record DrugDuplicationSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<ExactDuplicationRowReadModel> ExactActiveSubstanceDuplicates,
    ReportSubsectionReadModel<GroupDuplicationRowReadModel> ActiveSubstanceGroupDuplicates);

/// <summary>
/// Pregnancy-related main warning row [description].
/// </summary>
/// <param name="WarningCode">Warning code.</param>
/// <param name="DefaultText">Default text of the warning.</param>
public sealed record MainWarningReadModel(
    string WarningCode,
    string DefaultText);

/// <summary>
/// Database health condition row [description].
/// </summary>
/// <param name="HealthConditionId">Health condition identifier.</param>
/// <param name="HealthConditionName">Health condition name.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record HealthConditionRowReadModel(
    Guid HealthConditionId,
    string HealthConditionName,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Custom health condition row [description].
/// </summary>
/// <param name="CustomName">Custom health condition name.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record CustomHealthConditionRowReadModel(
    string CustomName,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Health conditions user-info group [description].
/// </summary>
/// <param name="FromDatabase">Health conditions from database.</param>
/// <param name="Custom">Custom health conditions.</param>
public sealed record HealthConditionsGroupReadModel(
    ReportSubsectionReadModel<HealthConditionRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomHealthConditionRowReadModel> Custom);

/// <summary>
/// Database problematic drug row [description].
/// </summary>
/// <param name="DrugId">Drug identifier.</param>
/// <param name="DrugFormId">Drug form identifier.</param>
/// <param name="DrugSummaryText">Drug summary text.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record ProblematicDrugRowReadModel(
    Guid DrugId,
    Guid DrugFormId,
    string DrugSummaryText,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Custom problematic drug row [description].
/// </summary>
/// <param name="CustomCommercialName">Custom commercial name.</param>
/// <param name="OtherCustomData">Other custom data.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record CustomProblematicDrugRowReadModel(
    string CustomCommercialName,
    string? OtherCustomData,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Taken-now drug overlap with a database drug problem [description].
/// </summary>
/// <param name="OverlapKind">0 = same catalog drug; 1 = shared active substance.</param>
/// <param name="RegularDrugId">Catalog drug identifier of the regular intake.</param>
/// <param name="RegularDrugFormId">Catalog drug form identifier of the regular intake.</param>
/// <param name="RegularDrugSummaryText">Regular intake drug summary text.</param>
/// <param name="ProblemDrugId">Catalog drug identifier of the problem record.</param>
/// <param name="ProblemDrugFormId">Catalog drug form identifier of the problem record.</param>
/// <param name="ProblemDrugSummaryText">Problem drug summary text.</param>
/// <param name="UserNote">Optional problem note.</param>
/// <param name="Title">Fixed title for the overlap kind.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="ActiveSubstanceId">Shared substance identifier when kind is 1.</param>
/// <param name="ActiveSubstanceName">Shared substance name when kind is 1.</param>
public sealed record DrugTakenNowProblemOverlapRowReadModel(
    DrugTakenNowAllergyOverlapKind OverlapKind,
    Guid RegularDrugId,
    Guid RegularDrugFormId,
    string RegularDrugSummaryText,
    Guid ProblemDrugId,
    Guid ProblemDrugFormId,
    string ProblemDrugSummaryText,
    string? UserNote,
    string Title,
    string DisplayLine,
    Guid? ActiveSubstanceId,
    string? ActiveSubstanceName);

/// <summary>
/// Problematic drugs user-info group [description].
/// </summary>
/// <param name="FromDatabase">Problematic drugs from database.</param>
/// <param name="Custom">Custom problematic drugs.</param>
/// <param name="TakenNowOverlap">Taken-now overlap subsection.</param>
public sealed record ProblematicDrugsGroupReadModel(
    ReportSubsectionReadModel<ProblematicDrugRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomProblematicDrugRowReadModel> Custom,
    ReportSubsectionReadModel<DrugTakenNowProblemOverlapRowReadModel> TakenNowOverlap);

/// <summary>
/// Drug allergy database row [description].
/// </summary>
/// <param name="DrugId">Drug identifier.</param>
/// <param name="DrugFormId">Drug form identifier.</param>
/// <param name="DrugSummaryText">Drug summary text.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record DrugAllergyRowReadModel(
    Guid DrugId,
    Guid DrugFormId,
    string DrugSummaryText,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Custom drug allergy row [description].
/// </summary>
/// <param name="CustomCommercialName">Custom commercial name.</param>
/// <param name="OtherCustomData">Other custom data.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record CustomDrugAllergyRowReadModel(
    string CustomCommercialName,
    string? OtherCustomData,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Catalog supplement allergy row [description].
/// </summary>
/// <param name="SupplementId">Supplement identifier.</param>
/// <param name="Name">Supplement name.</param>
/// <param name="AliasNames">Alias names.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record SupplementCatalogAllergyRowReadModel(
    Guid SupplementId,
    string Name,
    IReadOnlyCollection<string> AliasNames,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Custom supplement allergy row [description].
/// </summary>
/// <param name="Name">Supplement name.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record CustomSupplementAllergyRowReadModel(
    string Name,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Catalog food allergy row [description].
/// </summary>
/// <param name="FoodId">Food identifier.</param>
/// <param name="Name">Food name.</param>
/// <param name="AliasNames">Alias names.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record FoodCatalogAllergyRowReadModel(
    Guid FoodId,
    string Name,
    IReadOnlyCollection<string> AliasNames,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Custom food allergy row [description].
/// </summary>
/// <param name="Name">Food name.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record CustomFoodAllergyRowReadModel(
    string Name,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Taken-now supplement overlap with allergy row [description].
/// </summary>
/// <param name="RegularSupplementId">Regular supplement identifier.</param>
/// <param name="RegularIntakeName">Regular intake display name.</param>
/// <param name="AliasNames">Alias names.</param>
/// <param name="AllergyNote">Optional allergy note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record SupplementTakenNowAllergyOverlapRowReadModel(
    Guid RegularSupplementId,
    string RegularIntakeName,
    IReadOnlyCollection<string> AliasNames,
    string? AllergyNote,
    string DisplayLine);

/// <summary>
/// Taken-now food overlap with allergy row [description].
/// </summary>
/// <param name="RegularFoodId">Regular food identifier.</param>
/// <param name="RegularIntakeName">Regular intake display name.</param>
/// <param name="AliasNames">Alias names.</param>
/// <param name="AllergyNote">Optional allergy note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record FoodTakenNowAllergyOverlapRowReadModel(
    Guid RegularFoodId,
    string RegularIntakeName,
    IReadOnlyCollection<string> AliasNames,
    string? AllergyNote,
    string DisplayLine);

/// <summary>
/// Taken-now overlap between a regular catalog drug and a database drug allergy [description].
/// </summary>
public enum DrugTakenNowAllergyOverlapKind
{
    /// <summary>
    /// Same catalog drug; selected forms may differ.
    /// </summary>
    SameDrugName = 0,

    /// <summary>
    /// Different catalog drugs that share at least one active substance.
    /// </summary>
    SharedActiveSubstance = 1
}

/// <summary>
/// Taken-now drug overlap with allergy row [description].
/// </summary>
/// <param name="OverlapKind">0 = same catalog drug; 1 = shared active substance.</param>
/// <param name="RegularDrugId">Catalog drug identifier of the regular intake.</param>
/// <param name="RegularDrugFormId">Catalog drug form identifier of the regular intake.</param>
/// <param name="RegularDrugSummaryText">Regular intake drug summary text.</param>
/// <param name="AllergyDrugId">Catalog drug identifier of the allergy record.</param>
/// <param name="AllergyDrugFormId">Catalog drug form identifier of the allergy record.</param>
/// <param name="AllergyDrugSummaryText">Allergy drug summary text.</param>
/// <param name="UserNote">Optional allergy note.</param>
/// <param name="Title">Fixed title for the overlap kind.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="ActiveSubstanceId">Shared substance identifier when kind is 1.</param>
/// <param name="ActiveSubstanceName">Shared substance name when kind is 1.</param>
public sealed record DrugTakenNowAllergyOverlapRowReadModel(
    DrugTakenNowAllergyOverlapKind OverlapKind,
    Guid RegularDrugId,
    Guid RegularDrugFormId,
    string RegularDrugSummaryText,
    Guid AllergyDrugId,
    Guid AllergyDrugFormId,
    string AllergyDrugSummaryText,
    string? UserNote,
    string Title,
    string DisplayLine,
    Guid? ActiveSubstanceId,
    string? ActiveSubstanceName);

/// <summary>
/// Drug allergy subgroup [description].
/// </summary>
/// <param name="FromDatabase">Drug allergy database subsection.</param>
/// <param name="Custom">Custom drug allergy subsection.</param>
/// <param name="TakenNowOverlap">Taken-now overlap subsection.</param>
public sealed record AllergyDrugSubgroupReadModel(
    ReportSubsectionReadModel<DrugAllergyRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomDrugAllergyRowReadModel> Custom,
    ReportSubsectionReadModel<DrugTakenNowAllergyOverlapRowReadModel> TakenNowOverlap);

/// <summary>
/// Supplement allergy subgroup [description].
/// </summary>
/// <param name="FromDatabase">Supplement allergy database subsection.</param>
/// <param name="Custom">Custom supplement allergy subsection.</param>
/// <param name="TakenNowOverlap">Taken-now overlap subsection.</param>
public sealed record AllergySupplementSubgroupReadModel(
    ReportSubsectionReadModel<SupplementCatalogAllergyRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomSupplementAllergyRowReadModel> Custom,
    ReportSubsectionReadModel<SupplementTakenNowAllergyOverlapRowReadModel> TakenNowOverlap);

/// <summary>
/// Food allergy subgroup [description].
/// </summary>
/// <param name="FromDatabase">Food allergy database subsection.</param>
/// <param name="Custom">Custom food allergy subsection.</param>
/// <param name="TakenNowOverlap">Taken-now overlap subsection.</param>
public sealed record AllergyFoodSubgroupReadModel(
    ReportSubsectionReadModel<FoodCatalogAllergyRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomFoodAllergyRowReadModel> Custom,
    ReportSubsectionReadModel<FoodTakenNowAllergyOverlapRowReadModel> TakenNowOverlap);

/// <summary>
/// Other allergy catalog row [description].
/// </summary>
/// <param name="Name">Allergy name.</param>
/// <param name="DescriptionFromDb">Description from database.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record OtherAllergyCatalogRowReadModel(
    string Name,
    string? DescriptionFromDb,
    string DisplayLine);

/// <summary>
/// Custom other allergy row [description].
/// </summary>
/// <param name="Name">Allergy name.</param>
/// <param name="UserNote">Optional user note.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record CustomOtherAllergyRowReadModel(
    string Name,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Other allergy subgroup [description].
/// </summary>
/// <param name="FromDatabase">Other allergies from database.</param>
/// <param name="Custom">Custom other allergies.</param>
public sealed record AllergyOtherSubgroupReadModel(
    ReportSubsectionReadModel<OtherAllergyCatalogRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomOtherAllergyRowReadModel> Custom);

/// <summary>
/// Allergies user-info group [description].
/// </summary>
/// <param name="DrugAllergies">Drug allergy subgroup.</param>
/// <param name="SupplementAllergies">Supplement allergy subgroup.</param>
/// <param name="FoodAllergies">Food allergy subgroup.</param>
/// <param name="OtherAllergies">Other allergy subgroup.</param>
public sealed record AllergiesGroupReadModel(
    AllergyDrugSubgroupReadModel DrugAllergies,
    AllergySupplementSubgroupReadModel SupplementAllergies,
    AllergyFoodSubgroupReadModel FoodAllergies,
    AllergyOtherSubgroupReadModel OtherAllergies);

/// <summary>
/// CASE ONE user-info block [description].
/// </summary>
/// <param name="MainWarnings">Main warnings.</param>
/// <param name="HealthConditions">Health conditions group.</param>
/// <param name="ProblematicDrugs">Problematic drugs group.</param>
/// <param name="Allergies">Allergies group.</param>
public sealed record UserInfoSectionReadModel(
    IReadOnlyCollection<MainWarningReadModel> MainWarnings,
    HealthConditionsGroupReadModel HealthConditions,
    ProblematicDrugsGroupReadModel ProblematicDrugs,
    AllergiesGroupReadModel Allergies);

/// <summary>
/// Public report root (CASE TWO) [description].
/// </summary>
/// <param name="SettingsSnapshot">Settings snapshot for the report.</param>
/// <param name="SelectedDrugs">Selected drug summaries.</param>
/// <param name="SpecificDrugReports">Per-drug report blocks.</param>
/// <param name="AllDrugsInteractionsSection">Optional global interactions section.</param>
/// <param name="DrugDuplicationSection">Duplication warnings section.</param>
public sealed record PublicDrugInteractionToolReportReadModel(
    PublicDrugInteractionToolSettingsSnapshotReadModel SettingsSnapshot,
    IReadOnlyCollection<DrugTargetSummaryReadModel> SelectedDrugs,
    IReadOnlyCollection<PublicSpecificDrugReportReadModel> SpecificDrugReports,
    AllDrugsInteractionsSectionReadModel? AllDrugsInteractionsSection,
    DrugDuplicationSectionReadModel DrugDuplicationSection);

/// <summary>
/// Registered report root (CASE ONE) [description].
/// </summary>
/// <param name="SettingsSnapshot">Settings snapshot for the report.</param>
/// <param name="UserInfoSection">Optional user info section.</param>
/// <param name="SelectedDrugs">Selected drug summaries.</param>
/// <param name="SpecificDrugReports">Per-drug report blocks.</param>
/// <param name="AllDrugsInteractionsSection">Optional global interactions section.</param>
/// <param name="DrugDuplicationSection">Duplication warnings section.</param>
public sealed record RegisteredDrugInteractionToolReportReadModel(
    RegisteredDrugInteractionToolSettingsSnapshotReadModel SettingsSnapshot,
    UserInfoSectionReadModel? UserInfoSection,
    IReadOnlyCollection<DrugTargetSummaryReadModel> SelectedDrugs,
    IReadOnlyCollection<RegisteredSpecificDrugReportReadModel> SpecificDrugReports,
    AllDrugsInteractionsSectionReadModel? AllDrugsInteractionsSection,
    DrugDuplicationSectionReadModel DrugDuplicationSection);
