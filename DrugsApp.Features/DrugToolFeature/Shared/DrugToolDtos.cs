using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugToolFeature.Shared;

public sealed record DrugToolFormSummaryDto(
    string FormulationName,
    string Dosage,
    bool IsOral,
    IReadOnlyCollection<string> RoutesOfAdministration,
    IReadOnlyCollection<string> PharmaceuticalForms,
    string? Leaflet,
    string? EducationMaterials);

public sealed record DrugToolActiveSubstanceDto(
    Guid Id,
    string Name,
    string? OralGuidance);

public sealed record DrugToolAvailableDrugFormDto(
    Guid Id,
    string Label,
    bool IsOral,
    string Dosage,
    string FormulationName);

public sealed record DrugToolUserMealTimeDto(
    string MealName,
    string IntakeTime);

public sealed record DrugToolAlcoholRowDto(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    string DefaultText,
    string? SpecialText,
    string EffectiveTextSummary);

public sealed record DrugToolAlcoholSectionDto(
    int RecordCount,
    string Headline,
    IReadOnlyCollection<DrugToolAlcoholRowDto> Rows);

public sealed record DrugToolTargetFormDto(
    string FormulationName,
    string Dosage,
    bool IsOral);

public sealed record DrugToolDrugInteractionPairDto(
    string LeftActiveSubstanceName,
    string RightActiveSubstanceName,
    string DefaultText,
    string? StomachText,
    string Severity,
    Guid TargetDrugFormId,
    bool TargetDrugFormIsOral);

public sealed record DrugToolDrugInteractionGroupDto(
    Guid? TargetDrugId,
    string TargetCommercialName,
    string TargetDisplayLine,
    bool IsCurrentDrug,
    bool IsUserMatch,
    IReadOnlyCollection<DrugToolTargetFormDto> TargetForms,
    IReadOnlyCollection<DrugToolDrugInteractionPairDto> InteractionPairs);

public sealed record DrugToolInteractionSubsectionDto(
    string SubsectionCode,
    int RecordCount,
    IReadOnlyCollection<DrugToolDrugInteractionGroupDto> Groups);

public sealed record DrugToolDrugInteractionSectionDto(
    int RecordCount,
    IReadOnlyCollection<DrugToolInteractionSubsectionDto> Subsections);

public sealed record DrugToolFoodSupplementInteractionPairDto(
    string ActiveSubstanceName,
    string TargetDisplayName,
    string DefaultText,
    string? StomachText,
    string Severity);

public sealed record DrugToolFoodSupplementInteractionGroupDto(
    string TargetEntityType,
    string TargetDisplayName,
    bool IsUserMatch,
    IReadOnlyCollection<DrugToolFoodSupplementInteractionPairDto> InteractionPairs);

public sealed record DrugToolFoodSupplementSubsectionDto(
    string SubsectionCode,
    int RecordCount,
    IReadOnlyCollection<DrugToolFoodSupplementInteractionGroupDto> Groups);

public sealed record DrugToolFoodInteractionSectionDto(
    int RecordCount,
    DrugToolOralGuidanceDto? OralGuidance,
    IReadOnlyCollection<DrugToolFoodSupplementSubsectionDto> Subsections);

public sealed record DrugToolSupplementInteractionSectionDto(
    int RecordCount,
    IReadOnlyCollection<DrugToolFoodSupplementSubsectionDto> Subsections);

public sealed record DrugToolOralGuidanceSubstanceRowDto(
    string ActiveSubstanceName,
    string OralGuidance);

public sealed record DrugToolOralGuidanceDto(
    string Headline,
    string MealIntakeSummary,
    IReadOnlyCollection<DrugToolOralGuidanceSubstanceRowDto> ActiveSubstanceGuidanceRows);

public sealed record GetDrugToolDetailResponse(
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    Guid SelectedDrugFormId,
    DrugToolFormSummaryDto SelectedDrugFormSummary,
    string DrugDisplayLine,
    IReadOnlyCollection<DrugToolActiveSubstanceDto> ActiveSubstances,
    IReadOnlyCollection<DrugToolAvailableDrugFormDto> AvailableDrugForms,
    string CurrentInteractionScope,
    IReadOnlyCollection<string> AllowedInteractionScopes,
    IReadOnlyCollection<DrugToolUserMealTimeDto> UserMealTimes,
    DrugToolAlcoholSectionDto AlcoholSection,
    DrugToolDrugInteractionSectionDto DrugSection,
    DrugToolFoodInteractionSectionDto FoodSection,
    DrugToolSupplementInteractionSectionDto SupplementSection);
