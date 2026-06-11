using System.Text.Json.Serialization;
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
    string Name);

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

public sealed record DrugToolDrugInteractionPairDto(
    Guid ThisActiveSubstanceId,
    string ThisActiveSubstanceName,
    Guid TargetActiveSubstanceId,
    string TargetActiveSubstanceName,
    string DefaultText,
    string? StomachText,
    string Severity);

public sealed record DrugToolDrugTargetSummaryDto(
    Guid DrugId,
    Guid SelectedDrugFormId,
    string DrugSummaryText,
    bool IsOral);

public sealed record DrugToolDrugInteractionRowDto(
    DrugToolDrugTargetSummaryDto TargetDrug,
    int RecordCount,
    IReadOnlyCollection<DrugToolDrugInteractionPairDto> InteractionPairs);

public sealed record DrugToolDrugSubsectionDto(
    string SectionCode,
    string HeadlineText,
    int RecordCount,
    IReadOnlyCollection<DrugToolDrugInteractionRowDto> Rows);

public sealed record DrugToolDrugInteractionSectionDto(
    string Headline,
    IReadOnlyCollection<DrugToolDrugSubsectionDto>? TakenNow,
    IReadOnlyCollection<DrugToolDrugSubsectionDto>? AllOther);

public sealed record DrugToolFoodInteractionDto(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    Guid FoodId,
    string FoodName,
    string DefaultText,
    string? StomachText,
    string Severity);

public sealed record DrugToolFoodSubsectionDto(
    string SubsectionCode,
    int RecordCount,
    IReadOnlyCollection<DrugToolFoodInteractionDto> Interactions);

public sealed record DrugToolFoodInteractionSectionDto(
    string Headline,
    IReadOnlyCollection<DrugToolFoodSubsectionDto>? TakenNow,
    [property: JsonPropertyName("allother")]
    IReadOnlyCollection<DrugToolFoodSubsectionDto>? AllOther);

public sealed record DrugToolSupplementInteractionDto(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    Guid SupplementId,
    string SupplementName,
    string DefaultText,
    string? StomachText,
    string Severity);

public sealed record DrugToolSupplementSubsectionDto(
    string SubsectionCode,
    int RecordCount,
    IReadOnlyCollection<DrugToolSupplementInteractionDto> Interactions);

public sealed record DrugToolSupplementInteractionSectionDto(
    string Headline,
    IReadOnlyCollection<DrugToolSupplementSubsectionDto>? TakenNow,
    [property: JsonPropertyName("allother")]
    IReadOnlyCollection<DrugToolSupplementSubsectionDto>? AllOther);

public sealed record GetDrugToolDetailResponse(
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    Guid SelectedDrugFormId,
    DrugToolFormSummaryDto SelectedDrugFormSummary,
    IReadOnlyCollection<DrugToolActiveSubstanceDto> ActiveSubstances,
    string CurrentInteractionScope,
    IReadOnlyCollection<string> AllowedInteractionScopes,
    DrugToolAlcoholSectionDto AlcoholSection,
    DrugToolDrugInteractionSectionDto DrugSection,
    DrugToolFoodInteractionSectionDto FoodSection,
    DrugToolSupplementInteractionSectionDto SupplementSection);
