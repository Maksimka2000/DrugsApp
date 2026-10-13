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
/// <param name="Source">
/// Whether the drug was selected for the report, is taken regularly, or both; omitted in public reports,
/// where every drug is selected.
/// </param>
public sealed record DrugTargetSummaryReadModel(
    Guid DrugId,
    Guid DrugFormId,
    string DrugSummaryText,
    bool IsOral,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    SelectedDrugSource? Source = null);

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
/// Duplication-friendly drug summary without form id, listing each affected drug form [description].
/// </summary>
/// <param name="DrugId">Identifier of the drug.</param>
/// <param name="DrugFormId">Identifier of the affected drug form.</param>
/// <param name="OnlyDrugSummaryText">Drug summary as `Name (substances) | form | dosage`.</param>
/// <param name="IsOral">Indicates whether the form is oral.</param>
/// <param name="Source">Whether the drug is taken now or selected for this report.</param>
public sealed record DuplicationDrugSummaryReadModel(
    Guid DrugId,
    Guid DrugFormId,
    string OnlyDrugSummaryText,
    bool IsOral,
    DuplicationDrugSource Source);

/// <summary>
/// Regular intake schedule copied from the user's live regular drug, supplement, or food row [description].
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
/// <param name="SelectedSupplementCount">Number of supplements selected for the report.</param>
/// <param name="SelectedFoodCount">Number of foods selected for the report.</param>
/// <param name="Audience">Report audience.</param>
/// <param name="ShowPossibleInteractions">Whether catalog foods and supplements that are neither taken nor selected are analysed.</param>
public sealed record PublicDrugInteractionToolSettingsSnapshotReadModel(
    bool ShowAllDrugsInteractions,
    bool ShowOralGuidance,
    bool ShowAllFoodAndSupplementInteractions,
    int ManualDrugLimit,
    int SelectedSupplementCount = 0,
    int SelectedFoodCount = 0,
    ReportAudience Audience = ReportAudience.Public,
    bool ShowPossibleInteractions = true);

/// <summary>
/// Registered tool settings snapshot (CASE ONE) [description].
/// </summary>
/// <param name="IncludeUserData">Flag to include user data.</param>
/// <param name="ShowAllDrugsInteractions">Flag to include all drug interactions.</param>
/// <param name="ShowOralGuidance">Flag to include oral guidance.</param>
/// <param name="ShowAllFoodAndSupplementInteractions">Flag to include all food/supplement interactions.</param>
/// <param name="ManualDrugLimit">Maximum manual drug selection count.</param>
/// <param name="SelectedSupplementCount">Number of supplements selected for the report.</param>
/// <param name="SelectedFoodCount">Number of foods selected for the report.</param>
/// <param name="Audience">Report audience: registered client or healthcare provider.</param>
/// <param name="ShowPossibleInteractions">Whether catalog foods and supplements that are neither taken nor selected are analysed.</param>
public sealed record RegisteredDrugInteractionToolSettingsSnapshotReadModel(
    bool IncludeUserData,
    bool ShowAllDrugsInteractions,
    bool ShowOralGuidance,
    bool ShowAllFoodAndSupplementInteractions,
    int ManualDrugLimit,
    int SelectedSupplementCount = 0,
    int SelectedFoodCount = 0,
    ReportAudience Audience = ReportAudience.Registered,
    bool ShowPossibleInteractions = true);

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
/// <param name="AliasNames">Alternative food names; empty for food groups.</param>
/// <param name="InteractionId">Interaction identifier.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="StomachText">Optional stomach-related text.</param>
/// <param name="Severity">Interaction severity.</param>
/// <param name="IsTakenNow">Whether the food is taken now.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="Source">
/// <c>takingNow</c> in the taken-now subsection, <c>selected</c> in the selected subsection; omitted in the all-other subsection.
/// </param>
public sealed record FoodInteractionRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    Guid FoodId,
    string FoodName,
    IReadOnlyCollection<string> AliasNames,
    Guid InteractionId,
    string DefaultText,
    string? StomachText,
    string Severity,
    bool IsTakenNow,
    string DisplayLine,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    DuplicationDrugSource? Source = null);

/// <summary>
/// Supplement interaction row [description].
/// </summary>
/// <param name="ActiveSubstanceId">Active substance identifier.</param>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="SupplementId">Supplement identifier.</param>
/// <param name="SupplementName">Supplement name.</param>
/// <param name="AliasNames">Alternative supplement names; empty for supplement groups.</param>
/// <param name="InteractionId">Interaction identifier.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="StomachText">Optional stomach-related text.</param>
/// <param name="Severity">Interaction severity.</param>
/// <param name="IsTakenNow">Whether the supplement is taken now.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="Source">
/// <c>takingNow</c> in the taken-now subsection, <c>selected</c> in the selected subsection; omitted in the all-other subsection.
/// </param>
public sealed record SupplementInteractionRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    Guid SupplementId,
    string SupplementName,
    IReadOnlyCollection<string> AliasNames,
    Guid InteractionId,
    string DefaultText,
    string? StomachText,
    string Severity,
    bool IsTakenNow,
    string DisplayLine,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    DuplicationDrugSource? Source = null);

/// <summary>
/// Supplement interaction row reached through a catalog supplement that is a member of the user's own (custom) supplement [description].
/// </summary>
/// <param name="ActiveSubstanceId">Active substance identifier.</param>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="OwnSupplementId">Identifier of the own supplement that contains the interacting supplement.</param>
/// <param name="OwnSupplementName">Name of the own supplement that contains the interacting supplement.</param>
/// <param name="SupplementId">Identifier of the interacting catalog supplement inside the own supplement.</param>
/// <param name="SupplementName">Name of the interacting catalog supplement inside the own supplement.</param>
/// <param name="AliasNames">Alternative names of the interacting catalog supplement.</param>
/// <param name="InteractionId">Interaction identifier.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="StomachText">Optional stomach-related text.</param>
/// <param name="Severity">Interaction severity.</param>
/// <param name="IsTakenNow">Always true, because the own supplement is on the user's Active regular list.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="Source">Always <c>takingNow</c>, because the own supplement is on the user's regular list.</param>
public sealed record OwnSupplementInteractionRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    Guid OwnSupplementId,
    string OwnSupplementName,
    Guid SupplementId,
    string SupplementName,
    IReadOnlyCollection<string> AliasNames,
    Guid InteractionId,
    string DefaultText,
    string? StomachText,
    string Severity,
    bool IsTakenNow,
    string DisplayLine,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    DuplicationDrugSource? Source = null);

/// <summary>
/// Public per-drug food section (catalog allOther and selected foods) [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="AllOther">Subsection for all other interactions; null when all interactions are not requested.</param>
/// <param name="Selected">Interactions with the foods selected for the report; null when no food is selected.</param>
public sealed record PublicFoodInteractionSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<FoodInteractionRowReadModel>? AllOther,
    ReportSubsectionReadModel<FoodInteractionRowReadModel>? Selected = null);

/// <summary>
/// Public per-drug supplement section (catalog allOther and selected supplements) [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="AllOther">Subsection for all other interactions; null when all interactions are not requested.</param>
/// <param name="Selected">Interactions with the supplements selected for the report; null when no supplement is selected.</param>
public sealed record PublicSupplementInteractionSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel>? AllOther,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel>? Selected = null);

/// <summary>
/// Meal-time oral guidance subsection for registered reports [description].
/// </summary>
/// <param name="HeadlineText">Headline for the oral guidance block.</param>
/// <param name="MealIntakeSummary">Summary text for meal intake guidance.</param>
/// <param name="DrugIntakeTime">Daily clock times (HH:mm, comma-separated) the user takes this drug, or null when it is not on the regular list.</param>
/// <param name="ActiveSubstanceGuidanceRows">Guidance rows per active substance.</param>
public sealed record OralGuidanceSubsectionReadModel(
    string HeadlineText,
    string MealIntakeSummary,
    string? DrugIntakeTime,
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
/// <param name="Selected">Interactions with the foods selected for the report; null when no food is selected.</param>
public sealed record RegisteredFoodInteractionSectionReadModel(
    string SectionCode,
    OralGuidanceSubsectionReadModel? OralGuidance,
    ReportSubsectionReadModel<FoodInteractionRowReadModel>? TakenNow,
    ReportSubsectionReadModel<FoodInteractionRowReadModel>? AllOther,
    ReportSubsectionReadModel<FoodInteractionRowReadModel>? Selected = null);

/// <summary>
/// Registered per-drug supplement section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="TakenNow">Optional taken-now subsection.</param>
/// <param name="AllOther">Optional all-other subsection.</param>
/// <param name="OwnSupplement">Optional subsection for supplements inside the user's own supplements; present with user data.</param>
/// <param name="Selected">Interactions with the supplements selected for the report; null when no supplement is selected.</param>
public sealed record RegisteredSupplementInteractionSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel>? TakenNow,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel>? AllOther,
    ReportSubsectionReadModel<OwnSupplementInteractionRowReadModel>? OwnSupplement,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel>? Selected = null);

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
/// Same-drug duplication row: several forms of one drug are selected or taken together [description].
/// </summary>
/// <param name="DrugId">Identifier of the duplicated drug.</param>
/// <param name="DrugName">Commercial name of the duplicated drug.</param>
/// <param name="AffectedDrugs">Every affected drug form with its role (taking now or selected).</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="DuplicationKind">Duplication classification.</param>
public sealed record DrugDuplicationRowReadModel(
    Guid DrugId,
    string DrugName,
    IReadOnlyCollection<DuplicationDrugSummaryReadModel> AffectedDrugs,
    string DisplayLine,
    DuplicationKind DuplicationKind);

/// <summary>
/// Same-drug-form duplication row: a selected form is already on the user's regular intake list [description].
/// </summary>
/// <param name="DrugId">Identifier of the duplicated drug.</param>
/// <param name="DrugFormId">Identifier of the duplicated drug form.</param>
/// <param name="DrugSummaryText">Drug summary as `Name (substances) | form | dosage`.</param>
/// <param name="AffectedDrugs">The form in both of its roles (taking now and selected).</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="DuplicationKind">Duplication classification.</param>
public sealed record DrugFormDuplicationRowReadModel(
    Guid DrugId,
    Guid DrugFormId,
    string DrugSummaryText,
    IReadOnlyCollection<DuplicationDrugSummaryReadModel> AffectedDrugs,
    string DisplayLine,
    DuplicationKind DuplicationKind);

/// <summary>
/// Drug duplication warnings section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="ExactActiveSubstanceDuplicates">Active substances shared by different drugs.</param>
/// <param name="ActiveSubstanceGroupDuplicates">Active substance groups shared by different drugs.</param>
/// <param name="DrugDuplicates">Drugs selected or taken in more than one form.</param>
/// <param name="DrugFormDuplicates">Selected drug forms that are already taken regularly.</param>
/// <param name="SupplementDuplication">
/// Catalog supplements reaching the user from more than one source (selected, taken directly, inside own supplements);
/// omitted when food and supplement interactions are off.
/// </param>
public sealed record DrugDuplicationSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<ExactDuplicationRowReadModel> ExactActiveSubstanceDuplicates,
    ReportSubsectionReadModel<GroupDuplicationRowReadModel> ActiveSubstanceGroupDuplicates,
    ReportSubsectionReadModel<DrugDuplicationRowReadModel> DrugDuplicates,
    ReportSubsectionReadModel<DrugFormDuplicationRowReadModel> DrugFormDuplicates,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    ReportSubsectionReadModel<SupplementDuplicationRowReadModel>? SupplementDuplication);

/// <summary>
/// Catalog supplement taken regularly on its own, as one source of a supplement duplication [description].
/// </summary>
/// <param name="SupplementId">Catalog supplement identifier.</param>
/// <param name="SupplementSummaryText">Supplement name with aliases.</param>
/// <param name="Source">Role of the supplement (always taking now).</param>
public sealed record DuplicationSupplementSummaryReadModel(
    Guid SupplementId,
    string SupplementSummaryText,
    DuplicationDrugSource Source);

/// <summary>
/// Own supplement containing the duplicated catalog supplement, as one source of a supplement duplication [description].
/// </summary>
/// <param name="OwnSupplementId">Own supplement identifier.</param>
/// <param name="SupplementSummaryText">Own supplement as `Name | Contains: A, B`.</param>
/// <param name="Source">Role of the own supplement (always taking now).</param>
public sealed record DuplicationOwnSupplementSummaryReadModel(
    Guid OwnSupplementId,
    string SupplementSummaryText,
    DuplicationDrugSource Source);

/// <summary>
/// Supplement duplication row: one catalog supplement reaches the user through more than one regular source,
/// e.g. taken directly and inside an own supplement, or inside two own supplements [description].
/// </summary>
/// <param name="SupplementId">Duplicated catalog supplement identifier.</param>
/// <param name="SupplementSummaryText">Supplement name with aliases.</param>
/// <param name="AffectedSupplements">The supplement when it is taken regularly on its own; otherwise empty.</param>
/// <param name="AffectedOwnSupplements">Every regular own supplement containing the supplement.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
/// <param name="DuplicationKind">Duplication classification.</param>
public sealed record SupplementDuplicationRowReadModel(
    Guid SupplementId,
    string SupplementSummaryText,
    IReadOnlyCollection<DuplicationSupplementSummaryReadModel> AffectedSupplements,
    IReadOnlyCollection<DuplicationOwnSupplementSummaryReadModel> AffectedOwnSupplements,
    string DisplayLine,
    DuplicationKind DuplicationKind);

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
/// <param name="PlanTakingOverlap">Overlap subsection for manually selected drugs the user plans to take.</param>
public sealed record ProblematicDrugsGroupReadModel(
    ReportSubsectionReadModel<ProblematicDrugRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomProblematicDrugRowReadModel> Custom,
    ReportSubsectionReadModel<DrugTakenNowProblemOverlapRowReadModel> TakenNowOverlap,
    ReportSubsectionReadModel<DrugTakenNowProblemOverlapRowReadModel>? PlanTakingOverlap = null);

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
/// <param name="OwnSupplementId">Own supplement the allergen is taken through; null when taken directly.</param>
/// <param name="OwnSupplementName">Own supplement display name; null when taken directly.</param>
public sealed record SupplementTakenNowAllergyOverlapRowReadModel(
    Guid RegularSupplementId,
    string RegularIntakeName,
    IReadOnlyCollection<string> AliasNames,
    string? AllergyNote,
    string DisplayLine,
    Guid? OwnSupplementId = null,
    string? OwnSupplementName = null);

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
/// <param name="PlanTakingOverlap">Overlap subsection for manually selected drugs the user plans to take.</param>
public sealed record AllergyDrugSubgroupReadModel(
    ReportSubsectionReadModel<DrugAllergyRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomDrugAllergyRowReadModel> Custom,
    ReportSubsectionReadModel<DrugTakenNowAllergyOverlapRowReadModel> TakenNowOverlap,
    ReportSubsectionReadModel<DrugTakenNowAllergyOverlapRowReadModel>? PlanTakingOverlap = null);

/// <summary>
/// Supplement allergy subgroup [description].
/// </summary>
/// <param name="FromDatabase">Supplement allergy database subsection.</param>
/// <param name="Custom">Custom supplement allergy subsection.</param>
/// <param name="TakenNowOverlap">Overlap with supplements taken directly from the regular list.</param>
/// <param name="PlanTakingOverlap">Overlap with the supplements selected for the report (the user plans to take them).</param>
/// <param name="OwnSupplementOverlap">Overlap with supplements inside the user's regular own (custom) supplements.</param>
public sealed record AllergySupplementSubgroupReadModel(
    ReportSubsectionReadModel<SupplementCatalogAllergyRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomSupplementAllergyRowReadModel> Custom,
    ReportSubsectionReadModel<SupplementTakenNowAllergyOverlapRowReadModel> TakenNowOverlap,
    ReportSubsectionReadModel<SupplementTakenNowAllergyOverlapRowReadModel>? PlanTakingOverlap = null,
    ReportSubsectionReadModel<SupplementTakenNowAllergyOverlapRowReadModel>? OwnSupplementOverlap = null);

/// <summary>
/// Food allergy subgroup [description].
/// </summary>
/// <param name="FromDatabase">Food allergy database subsection.</param>
/// <param name="Custom">Custom food allergy subsection.</param>
/// <param name="TakenNowOverlap">Taken-now overlap subsection.</param>
/// <param name="PlanTakingOverlap">Overlap with the foods selected for the report (the user plans to take them).</param>
public sealed record AllergyFoodSubgroupReadModel(
    ReportSubsectionReadModel<FoodCatalogAllergyRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomFoodAllergyRowReadModel> Custom,
    ReportSubsectionReadModel<FoodTakenNowAllergyOverlapRowReadModel> TakenNowOverlap,
    ReportSubsectionReadModel<FoodTakenNowAllergyOverlapRowReadModel>? PlanTakingOverlap = null);

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
/// <param name="SupplementAllergies">Supplement allergy subgroup; omitted when food and supplement interactions are off.</param>
/// <param name="FoodAllergies">Food allergy subgroup; omitted when food and supplement interactions are off.</param>
/// <param name="OtherAllergies">Other allergy subgroup.</param>
public sealed record AllergiesGroupReadModel(
    AllergyDrugSubgroupReadModel DrugAllergies,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    AllergySupplementSubgroupReadModel? SupplementAllergies,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    AllergyFoodSubgroupReadModel? FoodAllergies,
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
/// Drug listed in the report info section, either taken now or selected for this report [description].
/// </summary>
/// <param name="DrugId">Identifier of the drug.</param>
/// <param name="DrugFormId">Identifier of the drug form.</param>
/// <param name="CommercialName">Commercial name of the drug.</param>
/// <param name="DrugSummaryText">Readable summary of the drug.</param>
/// <param name="FormulationName">Pharmaceutical formulation name of the form.</param>
/// <param name="Dosage">Dosage of the form.</param>
/// <param name="IsOral">Indicates whether the form is oral.</param>
/// <param name="ActiveSubstances">Active substances of the drug.</param>
/// <param name="RegularIntake">Schedule from the live regular-drug row; omitted for drugs that are only selected.</param>
public sealed record InfoDrugRowReadModel(
    Guid DrugId,
    Guid DrugFormId,
    string CommercialName,
    string DrugSummaryText,
    string FormulationName,
    string Dosage,
    bool IsOral,
    IReadOnlyCollection<ActiveSubstanceRefReadModel> ActiveSubstances,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    RegularDrugIntakeReadModel? RegularIntake);

/// <summary>
/// Catalog supplement the user takes now, listed in the report info section [description].
/// </summary>
/// <param name="SupplementId">Identifier of the catalog supplement.</param>
/// <param name="SupplementName">Name of the catalog supplement.</param>
/// <param name="AliasNames">Alternative names of the catalog supplement.</param>
/// <param name="RegularIntake">Schedule from the live regular-supplement row.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record InfoSupplementRowReadModel(
    Guid SupplementId,
    string SupplementName,
    IReadOnlyCollection<string> AliasNames,
    RegularDrugIntakeReadModel RegularIntake,
    string DisplayLine);

/// <summary>
/// Catalog supplement contained in an own supplement listed in the report info section [description].
/// </summary>
/// <param name="SupplementId">Identifier of the catalog supplement.</param>
/// <param name="SupplementName">Name of the catalog supplement.</param>
/// <param name="AliasNames">Alternative names of the catalog supplement.</param>
public sealed record InfoOwnSupplementMemberReadModel(
    Guid SupplementId,
    string SupplementName,
    IReadOnlyCollection<string> AliasNames);

/// <summary>
/// Own (custom) supplement the user takes now, listed with its catalog supplements in the report info section [description].
/// </summary>
/// <param name="OwnSupplementId">Identifier of the own supplement.</param>
/// <param name="OwnSupplementName">Name of the own supplement.</param>
/// <param name="MemberSupplements">Catalog supplements inside the own supplement, in label order.</param>
/// <param name="RegularIntake">Schedule from the live regular-supplement row.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record InfoOwnSupplementRowReadModel(
    Guid OwnSupplementId,
    string OwnSupplementName,
    IReadOnlyCollection<InfoOwnSupplementMemberReadModel> MemberSupplements,
    RegularDrugIntakeReadModel RegularIntake,
    string DisplayLine);

/// <summary>
/// Catalog food the user takes now, listed in the report info section [description].
/// </summary>
/// <param name="FoodId">Identifier of the catalog food.</param>
/// <param name="FoodName">Name of the catalog food.</param>
/// <param name="AliasNames">Alternative names of the catalog food.</param>
/// <param name="RegularIntake">Schedule from the live regular-food row.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record InfoFoodRowReadModel(
    Guid FoodId,
    string FoodName,
    IReadOnlyCollection<string> AliasNames,
    RegularDrugIntakeReadModel RegularIntake,
    string DisplayLine);

/// <summary>
/// Catalog supplement or food selected for this report (for a provider: selected for prescription) [description].
/// </summary>
/// <param name="Id">Identifier of the catalog supplement or food.</param>
/// <param name="Name">Name of the catalog supplement or food.</param>
/// <param name="AliasNames">Alternative names.</param>
/// <param name="IsAlsoTakenNow">
/// True when the user already takes it regularly; null without user data.
/// </param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record InfoSelectedItemRowReadModel(
    Guid Id,
    string Name,
    IReadOnlyCollection<string> AliasNames,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? IsAlsoTakenNow,
    string DisplayLine);

/// <summary>
/// Own (custom) supplement reference [description].
/// </summary>
/// <param name="OwnSupplementId">Own supplement identifier.</param>
/// <param name="OwnSupplementName">Own supplement name.</param>
public sealed record InfoOwnSupplementRefReadModel(
    Guid OwnSupplementId,
    string OwnSupplementName);

/// <summary>
/// Supplement that is selected for the report and already taken regularly, directly and/or inside own supplements [description].
/// </summary>
/// <param name="SupplementId">Catalog supplement identifier.</param>
/// <param name="SupplementName">Catalog supplement name.</param>
/// <param name="AliasNames">Alternative names.</param>
/// <param name="IsTakenDirectly">True when the supplement is on the regular list on its own.</param>
/// <param name="OwnSupplements">Regular own supplements that contain the supplement.</param>
/// <param name="RegularIntake">Schedule of the direct regular intake; null when it is taken only inside own supplements.</param>
/// <param name="DisplayLine">Display-friendly line.</param>
public sealed record InfoSupplementSelectedTwiceRowReadModel(
    Guid SupplementId,
    string SupplementName,
    IReadOnlyCollection<string> AliasNames,
    bool IsTakenDirectly,
    IReadOnlyCollection<InfoOwnSupplementRefReadModel> OwnSupplements,
    RegularDrugIntakeReadModel? RegularIntake,
    string DisplayLine);

/// <summary>
/// Drugs part of the report info section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="TakenNow">Active regular drugs merged into the report; null without user data.</param>
/// <param name="Selected">Drugs selected manually for this report.</param>
/// <param name="SelectedTwice">
/// Drugs both selected manually and already taken regularly, with their regular intake; null without user data.
/// </param>
public sealed record InfoDrugsSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<InfoDrugRowReadModel>? TakenNow,
    ReportSubsectionReadModel<InfoDrugRowReadModel> Selected,
    ReportSubsectionReadModel<InfoDrugRowReadModel>? SelectedTwice);

/// <summary>
/// Supplements part of the report info section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="TakenNow">Active regular catalog supplements; null without user data.</param>
/// <param name="OwnSupplements">Active regular own supplements with their members; null without user data.</param>
/// <param name="Selected">Catalog supplements selected for this report.</param>
/// <param name="SelectedTwice">
/// Selected supplements that are also taken regularly (directly or inside own supplements); null without user data.
/// </param>
public sealed record InfoSupplementsSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<InfoSupplementRowReadModel>? TakenNow,
    ReportSubsectionReadModel<InfoOwnSupplementRowReadModel>? OwnSupplements,
    ReportSubsectionReadModel<InfoSelectedItemRowReadModel> Selected,
    ReportSubsectionReadModel<InfoSupplementSelectedTwiceRowReadModel>? SelectedTwice);

/// <summary>
/// Foods part of the report info section [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="TakenNow">Active regular foods; null without user data.</param>
/// <param name="Selected">Catalog foods selected for this report.</param>
public sealed record InfoFoodsSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<InfoFoodRowReadModel>? TakenNow,
    ReportSubsectionReadModel<InfoSelectedItemRowReadModel> Selected);

/// <summary>
/// Everything the report was computed from: drugs taken now and selected, supplements and own supplements taken now, and foods taken now [description].
/// </summary>
/// <param name="SectionCode">Section code identifier.</param>
/// <param name="Drugs">Drugs taken now and selected.</param>
/// <param name="Supplements">Supplements taken now and selected.</param>
/// <param name="Foods">Foods taken now and selected.</param>
public sealed record ReportInfoSectionReadModel(
    string SectionCode,
    InfoDrugsSectionReadModel Drugs,
    InfoSupplementsSectionReadModel? Supplements,
    InfoFoodsSectionReadModel? Foods);

/// <summary>
/// Public report root (CASE TWO) [description].
/// </summary>
/// <param name="SettingsSnapshot">Settings snapshot for the report.</param>
/// <param name="Info">Drugs the report was computed from.</param>
/// <param name="SpecificDrugReports">Per-drug report blocks.</param>
/// <param name="AllDrugsInteractionsSection">Optional global interactions section.</param>
/// <param name="DrugDuplicationSection">Duplication warnings section.</param>
public sealed record PublicDrugInteractionToolReportReadModel(
    PublicDrugInteractionToolSettingsSnapshotReadModel SettingsSnapshot,
    ReportInfoSectionReadModel Info,
    IReadOnlyCollection<PublicSpecificDrugReportReadModel> SpecificDrugReports,
    AllDrugsInteractionsSectionReadModel? AllDrugsInteractionsSection,
    DrugDuplicationSectionReadModel DrugDuplicationSection);

/// <summary>
/// Registered report root (CASE ONE) [description].
/// </summary>
/// <param name="SettingsSnapshot">Settings snapshot for the report.</param>
/// <param name="UserInfoSection">Optional user info section.</param>
/// <param name="Info">Drugs, supplements, own supplements, and foods the report was computed from.</param>
/// <param name="SpecificDrugReports">Per-drug report blocks.</param>
/// <param name="AllDrugsInteractionsSection">Optional global interactions section.</param>
/// <param name="DrugDuplicationSection">Duplication warnings section.</param>
public sealed record RegisteredDrugInteractionToolReportReadModel(
    RegisteredDrugInteractionToolSettingsSnapshotReadModel SettingsSnapshot,
    UserInfoSectionReadModel? UserInfoSection,
    ReportInfoSectionReadModel Info,
    IReadOnlyCollection<RegisteredSpecificDrugReportReadModel> SpecificDrugReports,
    AllDrugsInteractionsSectionReadModel? AllDrugsInteractionsSection,
    DrugDuplicationSectionReadModel DrugDuplicationSection);
