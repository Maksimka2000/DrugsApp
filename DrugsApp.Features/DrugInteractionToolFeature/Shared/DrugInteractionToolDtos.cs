namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Generic foldable subsection wrapper for report UI [description].
/// </summary>
public sealed record ReportSubsectionReadModel<TRow>(
    string SectionCode,
    string HeadlineText,
    int RecordCount,
    IReadOnlyCollection<TRow> Rows);

/// <summary>
/// Active substance line embedded in a drug header [description].
/// </summary>
public sealed record ActiveSubstanceInDrugReadModel(
    Guid ActiveSubstanceId,
    string Name,
    bool IsOralInSelectedForm,
    string? OralGuidanceText);

/// <summary>
/// Shared drug header summary for selected and compared drugs [description].
/// </summary>
public sealed record SelectedDrugSummaryReadModel(
    Guid DrugFormId,
    Guid DrugId,
    string CommercialName,
    IReadOnlyCollection<ActiveSubstanceInDrugReadModel> ActiveSubstances,
    string FormulationName,
    string Dosage,
    bool IsOral,
    string? TypeOfDrug,
    string? TypeOfProduct,
    SelectedDrugSource? Source,
    string DisplayLine);

/// <summary>
/// Public tool settings snapshot (CASE TWO) [description].
/// </summary>
public sealed record PublicDrugInteractionToolSettingsSnapshotReadModel(
    bool ShowAllDrugsInteractions,
    bool ShowOralGuidance,
    bool ShowAllFoodAndSupplementInteractions,
    int ManualDrugLimit);

/// <summary>
/// Registered tool settings snapshot (CASE ONE) [description].
/// </summary>
public sealed record RegisteredDrugInteractionToolSettingsSnapshotReadModel(
    bool IncludeUserData,
    bool ShowAllDrugsInteractions,
    bool ShowOralGuidance,
    bool ShowAllFoodAndSupplementInteractions,
    int ManualDrugLimit);

/// <summary>
/// Substance pair interaction row reused across per-drug and global sections [description].
/// </summary>
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
public sealed record ComparedDrugInteractionGroupReadModel(
    string GroupTitle,
    SelectedDrugSummaryReadModel CurrentDrug,
    SelectedDrugSummaryReadModel ComparedDrug,
    int RecordCount,
    IReadOnlyCollection<SubstancePairInteractionRowReadModel> PairRows);

/// <summary>
/// Per-drug cross-drug interaction section [description].
/// </summary>
public sealed record DrugInteractionSectionReadModel(
    string SectionCode,
    int RecordCount,
    IReadOnlyCollection<ComparedDrugInteractionGroupReadModel> ComparedDrugGroups);

/// <summary>
/// Alcohol interaction row for a single active substance [description].
/// </summary>
public sealed record AlcoholInteractionRowReadModel(
    string ActiveSubstanceName,
    string DefaultText,
    string? SpecialText);

/// <summary>
/// Per-drug alcohol interaction section [description].
/// </summary>
public sealed record AlcoholInteractionSectionReadModel(
    string HeadlineText,
    IReadOnlyCollection<AlcoholInteractionRowReadModel> Rows);

/// <summary>
/// Food or supplement interaction row [description].
/// </summary>
public sealed record FoodInteractionRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    InteractionPartyKind PartyKind,
    Guid PartyId,
    string DisplayName,
    Guid InteractionId,
    string DefaultText,
    string? StomachText,
    string Severity,
    bool IsTakenNow,
    string DisplayLine);

/// <summary>
/// Supplement interaction row [description].
/// </summary>
public sealed record SupplementInteractionRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    InteractionPartyKind PartyKind,
    Guid PartyId,
    string DisplayName,
    Guid InteractionId,
    string DefaultText,
    string? StomachText,
    string Severity,
    bool IsTakenNow,
    string DisplayLine);

/// <summary>
/// Public per-drug food section (catalog allOther only) [description].
/// </summary>
public sealed record PublicFoodInteractionSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<FoodInteractionRowReadModel> AllOther);

/// <summary>
/// Public per-drug supplement section (catalog allOther only) [description].
/// </summary>
public sealed record PublicSupplementInteractionSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel> AllOther);

/// <summary>
/// Meal-time oral guidance subsection for registered reports [description].
/// </summary>
public sealed record OralGuidanceSubsectionReadModel(
    string HeadlineText,
    string MealIntakeSummary,
    IReadOnlyCollection<ActiveSubstanceOralGuidanceRowReadModel> ActiveSubstanceGuidanceRows);

/// <summary>
/// Per-substance oral guidance inside meal-time block [description].
/// </summary>
public sealed record ActiveSubstanceOralGuidanceRowReadModel(
    string ActiveSubstanceName,
    string GuidanceText,
    string DisplayLine);

/// <summary>
/// Registered per-drug food section [description].
/// </summary>
public sealed record RegisteredFoodInteractionSectionReadModel(
    string SectionCode,
    OralGuidanceSubsectionReadModel? OralGuidance,
    ReportSubsectionReadModel<FoodInteractionRowReadModel>? TakenNow,
    ReportSubsectionReadModel<FoodInteractionRowReadModel>? AllOther);

/// <summary>
/// Registered per-drug supplement section [description].
/// </summary>
public sealed record RegisteredSupplementInteractionSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel>? TakenNow,
    ReportSubsectionReadModel<SupplementInteractionRowReadModel>? AllOther);

/// <summary>
/// Per-drug problematic drug row [description].
/// </summary>
public sealed record SpecificDrugProblemRowReadModel(
    string ProblemDescription,
    string HeadlineText);

/// <summary>
/// Per-drug problems section [description].
/// </summary>
public sealed record SpecificDrugProblemsSectionReadModel(
    string SectionCode,
    int RecordCount,
    IReadOnlyCollection<SpecificDrugProblemRowReadModel> Rows);

/// <summary>
/// Direct drug allergy row on a specific drug [description].
/// </summary>
public sealed record SpecificDrugDirectAllergyRowReadModel(
    string ProblemDescription,
    string HeadlineText);

/// <summary>
/// Active substance overlap allergy row on a specific drug [description].
/// </summary>
public sealed record ActiveSubstanceAllergyOverlapRowReadModel(
    string ActiveSubstanceName,
    string SourceAllergyDrugDisplay,
    string? UserNote,
    string HeadlineText,
    string DisplayLine);

/// <summary>
/// Per-drug allergies section [description].
/// </summary>
public sealed record SpecificDrugAllergiesSectionReadModel(
    string SectionCode,
    int RecordCount,
    SpecificDrugDirectAllergyRowReadModel? DirectDrugAllergy,
    IReadOnlyCollection<ActiveSubstanceAllergyOverlapRowReadModel> ActiveSubstanceOverlapRows);

/// <summary>
/// Public per-drug report block (CASE TWO) [description].
/// </summary>
public sealed record PublicSpecificDrugReportReadModel(
    SelectedDrugSummaryReadModel Drug,
    AlcoholInteractionSectionReadModel Alcohol,
    DrugInteractionSectionReadModel Drugs,
    PublicFoodInteractionSectionReadModel? Foods,
    PublicSupplementInteractionSectionReadModel? Supplements);

/// <summary>
/// Registered per-drug report block (CASE ONE) [description].
/// </summary>
public sealed record RegisteredSpecificDrugReportReadModel(
    SelectedDrugSummaryReadModel Drug,
    SpecificDrugProblemsSectionReadModel? Problems,
    SpecificDrugAllergiesSectionReadModel? Allergies,
    AlcoholInteractionSectionReadModel Alcohol,
    DrugInteractionSectionReadModel Drugs,
    RegisteredFoodInteractionSectionReadModel? Foods,
    RegisteredSupplementInteractionSectionReadModel? Supplements);

/// <summary>
/// Global drug pair group at report bottom [description].
/// </summary>
public sealed record GlobalDrugPairGroupReadModel(
    string GroupTitle,
    SelectedDrugSummaryReadModel LeftDrug,
    SelectedDrugSummaryReadModel RightDrug,
    int RecordCount,
    IReadOnlyCollection<SubstancePairInteractionRowReadModel> PairRows);

/// <summary>
/// Global all-drugs interaction matrix section [description].
/// </summary>
public sealed record AllDrugsInteractionsSectionReadModel(
    string SectionCode,
    int RecordCount,
    IReadOnlyCollection<GlobalDrugPairGroupReadModel> PairGroups);

/// <summary>
/// Exact active substance duplication row [description].
/// </summary>
public sealed record ExactDuplicationRowReadModel(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    IReadOnlyCollection<SelectedDrugSummaryReadModel> AffectedDrugs,
    string DisplayLine,
    DuplicationKind DuplicationKind);

/// <summary>
/// Active substance group duplication row [description].
/// </summary>
public sealed record GroupDuplicationRowReadModel(
    Guid ActiveSubstanceGroupId,
    string GroupName,
    IReadOnlyCollection<SelectedDrugSummaryReadModel> AffectedDrugs,
    string DisplayLine,
    DuplicationKind DuplicationKind);

/// <summary>
/// Drug duplication warnings section [description].
/// </summary>
public sealed record DrugDuplicationSectionReadModel(
    string SectionCode,
    ReportSubsectionReadModel<ExactDuplicationRowReadModel> ExactActiveSubstanceDuplicates,
    ReportSubsectionReadModel<GroupDuplicationRowReadModel> ActiveSubstanceGroupDuplicates);

/// <summary>
/// Pregnancy-related main warning row [description].
/// </summary>
public sealed record MainWarningReadModel(
    string WarningCode,
    string DefaultText);

/// <summary>
/// Database health condition row [description].
/// </summary>
public sealed record HealthConditionRowReadModel(
    string HealthConditionName,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Custom health condition row [description].
/// </summary>
public sealed record CustomHealthConditionRowReadModel(
    string CustomName,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Health conditions user-info group [description].
/// </summary>
public sealed record HealthConditionsGroupReadModel(
    ReportSubsectionReadModel<HealthConditionRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomHealthConditionRowReadModel> Custom);

/// <summary>
/// Database problematic drug row [description].
/// </summary>
public sealed record ProblematicDrugRowReadModel(
    string DrugDisplayLine,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Custom problematic drug row [description].
/// </summary>
public sealed record CustomProblematicDrugRowReadModel(
    string CommercialName,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Problematic drugs user-info group [description].
/// </summary>
public sealed record ProblematicDrugsGroupReadModel(
    ReportSubsectionReadModel<ProblematicDrugRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomProblematicDrugRowReadModel> Custom);

/// <summary>
/// Drug allergy database row [description].
/// </summary>
public sealed record DrugAllergyRowReadModel(
    string DrugDisplayLine,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Custom drug allergy row [description].
/// </summary>
public sealed record CustomDrugAllergyRowReadModel(
    string CommercialName,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Catalog allergy row for food or supplement [description].
/// </summary>
public sealed record CatalogAllergyRowReadModel(
    string Name,
    IReadOnlyCollection<string> AliasNames,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Custom catalog allergy row [description].
/// </summary>
public sealed record CustomCatalogAllergyRowReadModel(
    string Name,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Taken-now overlap with allergy row [description].
/// </summary>
public sealed record TakenNowAllergyOverlapRowReadModel(
    string RegularIntakeName,
    IReadOnlyCollection<string> AliasNames,
    string? AllergyNote,
    string DisplayLine);

/// <summary>
/// Drug allergy subgroup [description].
/// </summary>
public sealed record AllergyDrugSubgroupReadModel(
    ReportSubsectionReadModel<DrugAllergyRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomDrugAllergyRowReadModel> Custom);

/// <summary>
/// Supplement allergy subgroup [description].
/// </summary>
public sealed record AllergySupplementSubgroupReadModel(
    ReportSubsectionReadModel<CatalogAllergyRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomCatalogAllergyRowReadModel> Custom,
    ReportSubsectionReadModel<TakenNowAllergyOverlapRowReadModel> TakenNowOverlap);

/// <summary>
/// Food allergy subgroup [description].
/// </summary>
public sealed record AllergyFoodSubgroupReadModel(
    ReportSubsectionReadModel<CatalogAllergyRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomCatalogAllergyRowReadModel> Custom,
    ReportSubsectionReadModel<TakenNowAllergyOverlapRowReadModel> TakenNowOverlap);

/// <summary>
/// Other allergy catalog row [description].
/// </summary>
public sealed record OtherAllergyCatalogRowReadModel(
    string Name,
    string? DescriptionFromDb,
    string DisplayLine);

/// <summary>
/// Custom other allergy row [description].
/// </summary>
public sealed record CustomOtherAllergyRowReadModel(
    string Name,
    string? UserNote,
    string DisplayLine);

/// <summary>
/// Other allergy subgroup [description].
/// </summary>
public sealed record AllergyOtherSubgroupReadModel(
    ReportSubsectionReadModel<OtherAllergyCatalogRowReadModel> FromDatabase,
    ReportSubsectionReadModel<CustomOtherAllergyRowReadModel> Custom);

/// <summary>
/// Allergies user-info group [description].
/// </summary>
public sealed record AllergiesGroupReadModel(
    AllergyDrugSubgroupReadModel DrugAllergies,
    AllergySupplementSubgroupReadModel SupplementAllergies,
    AllergyFoodSubgroupReadModel FoodAllergies,
    AllergyOtherSubgroupReadModel OtherAllergies);

/// <summary>
/// CASE ONE user-info block [description].
/// </summary>
public sealed record UserInfoSectionReadModel(
    IReadOnlyCollection<MainWarningReadModel> MainWarnings,
    HealthConditionsGroupReadModel HealthConditions,
    ProblematicDrugsGroupReadModel ProblematicDrugs,
    AllergiesGroupReadModel Allergies);

/// <summary>
/// Public report root (CASE TWO) [description].
/// </summary>
public sealed record PublicDrugInteractionToolReportReadModel(
    PublicDrugInteractionToolSettingsSnapshotReadModel SettingsSnapshot,
    IReadOnlyCollection<SelectedDrugSummaryReadModel> SelectedDrugs,
    IReadOnlyCollection<PublicSpecificDrugReportReadModel> SpecificDrugReports,
    AllDrugsInteractionsSectionReadModel? AllDrugsInteractionsSection,
    DrugDuplicationSectionReadModel DrugDuplicationSection);

/// <summary>
/// Registered report root (CASE ONE) [description].
/// </summary>
public sealed record RegisteredDrugInteractionToolReportReadModel(
    RegisteredDrugInteractionToolSettingsSnapshotReadModel SettingsSnapshot,
    UserInfoSectionReadModel? UserInfoSection,
    IReadOnlyCollection<SelectedDrugSummaryReadModel> SelectedDrugs,
    IReadOnlyCollection<RegisteredSpecificDrugReportReadModel> SpecificDrugReports,
    AllDrugsInteractionsSectionReadModel? AllDrugsInteractionsSection,
    DrugDuplicationSectionReadModel DrugDuplicationSection);
