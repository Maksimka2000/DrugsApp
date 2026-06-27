using System.Text.Json.Serialization;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugToolFeature.Shared;

/// <summary>
/// Summary of a selected drug form for interaction tool responses.
/// </summary>
/// <param name="FormulationName">Readable pharmaceutical formulation name.</param>
/// <param name="Dosage">Dosage description.</param>
/// <param name="IsOral">Flag indicating if form is oral.</param>
/// <param name="RoutesOfAdministration">Supported administration routes.</param>
/// <param name="PharmaceuticalForms">List of available pharmaceutical forms.</param>
/// <param name="Leaflet">Link or text for the leaflet.</param>
/// <param name="EducationMaterials">Supplementary education materials.</param>
public sealed record DrugToolFormSummaryDto(
    string FormulationName,
    string Dosage,
    bool IsOral,
    IReadOnlyCollection<string> RoutesOfAdministration,
    IReadOnlyCollection<string> PharmaceuticalForms,
    string? Leaflet,
    string? EducationMaterials);

/// <summary>
/// Active substance reference used in interaction tool responses.
/// </summary>
/// <param name="Id">Identifier of the active substance.</param>
/// <param name="Name">Display name of the active substance.</param>
public sealed record DrugToolActiveSubstanceDto(
    Guid Id,
    string Name);

/// <summary>
/// Represents a single alcohol interaction row.
/// </summary>
/// <param name="ActiveSubstanceId">Identifier of the active substance.</param>
/// <param name="ActiveSubstanceName">Name of the active substance.</param>
/// <param name="DefaultText">Default interaction description.</param>
/// <param name="SpecialText">Optional special interaction note.</param>
/// <param name="EffectiveTextSummary">Effective summary text after processing.</param>
public sealed record DrugToolAlcoholRowDto(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    string DefaultText,
    string? SpecialText,
    string EffectiveTextSummary);

/// <summary>
/// Represents the alcohol interaction section.
/// </summary>
/// <param name="RecordCount">Total number of rows.</param>
/// <param name="Headline">Section headline.</param>
/// <param name="Rows">Collection of alcohol interaction rows.</param>
public sealed record DrugToolAlcoholSectionDto(
    int RecordCount,
    string Headline,
    IReadOnlyCollection<DrugToolAlcoholRowDto> Rows);

/// <summary>
/// Drug-to-drug interaction pair within a subsection.
/// </summary>
/// <param name="ThisActiveSubstanceId">Current drug active substance id.</param>
/// <param name="ThisActiveSubstanceName">Current drug active substance name.</param>
/// <param name="TargetActiveSubstanceId">Target drug active substance id.</param>
/// <param name="TargetActiveSubstanceName">Target drug active substance name.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="StomachText">Optional stomach-related note.</param>
/// <param name="Severity">Interaction severity label.</param>
public sealed record DrugToolDrugInteractionPairDto(
    Guid ThisActiveSubstanceId,
    string ThisActiveSubstanceName,
    Guid TargetActiveSubstanceId,
    string TargetActiveSubstanceName,
    string DefaultText,
    string? StomachText,
    string Severity);

/// <summary>
/// Summary of a target drug used in drug interaction sections.
/// </summary>
/// <param name="DrugId">Drug identifier.</param>
/// <param name="SelectedDrugFormId">Selected drug form identifier.</param>
/// <param name="DrugSummaryText">Readable summary for the drug.</param>
/// <param name="IsOral">Flag indicating if the form is oral.</param>
public sealed record DrugToolDrugTargetSummaryDto(
    Guid DrugId,
    Guid SelectedDrugFormId,
    string DrugSummaryText,
    bool IsOral);

/// <summary>
/// Aggregates drug interaction pairs for a single target drug.
/// </summary>
/// <param name="TargetDrug">Target drug summary.</param>
/// <param name="RecordCount">Number of interaction pairs.</param>
/// <param name="InteractionPairs">Collection of interaction pairs.</param>
public sealed record DrugToolDrugInteractionRowDto(
    DrugToolDrugTargetSummaryDto TargetDrug,
    int RecordCount,
    IReadOnlyCollection<DrugToolDrugInteractionPairDto> InteractionPairs);

/// <summary>
/// Drug interaction subsection grouped by code and headline.
/// </summary>
/// <param name="SectionCode">Subsection code identifier.</param>
/// <param name="HeadlineText">Subsection headline.</param>
/// <param name="RecordCount">Total rows.</param>
/// <param name="Rows">Rows contained in this subsection.</param>
public sealed record DrugToolDrugSubsectionDto(
    string SectionCode,
    string HeadlineText,
    int RecordCount,
    IReadOnlyCollection<DrugToolDrugInteractionRowDto> Rows);

/// <summary>
/// Drug interaction section separating taken-now and all-other subsections.
/// </summary>
/// <param name="Headline">Section headline.</param>
/// <param name="TakenNow">Interactions for drugs taken now.</param>
/// <param name="AllOther">Other interaction subsections.</param>
public sealed record DrugToolDrugInteractionSectionDto(
    string Headline,
    IReadOnlyCollection<DrugToolDrugSubsectionDto>? TakenNow,
    IReadOnlyCollection<DrugToolDrugSubsectionDto>? AllOther);

/// <summary>
/// Food interaction entry for the drug tool.
/// </summary>
/// <param name="ActiveSubstanceId">Active substance identifier.</param>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="FoodId">Food identifier.</param>
/// <param name="FoodName">Food name.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="StomachText">Optional stomach-related text.</param>
/// <param name="Severity">Severity label.</param>
public sealed record DrugToolFoodInteractionDto(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    Guid FoodId,
    string FoodName,
    string DefaultText,
    string? StomachText,
    string Severity);

/// <summary>
/// Food interaction subsection grouped by subsection code.
/// </summary>
/// <param name="SubsectionCode">Subsection code identifier.</param>
/// <param name="RecordCount">Total interactions in the subsection.</param>
/// <param name="Interactions">Collection of interactions.</param>
public sealed record DrugToolFoodSubsectionDto(
    string SubsectionCode,
    int RecordCount,
    IReadOnlyCollection<DrugToolFoodInteractionDto> Interactions);

/// <summary>
/// Food interaction section containing taken-now and all-other subsections.
/// </summary>
/// <param name="Headline">Section headline.</param>
/// <param name="TakenNow">Interactions for items taken now.</param>
/// <param name="AllOther">Other interactions (serialized as allother).</param>
public sealed record DrugToolFoodInteractionSectionDto(
    string Headline,
    IReadOnlyCollection<DrugToolFoodSubsectionDto>? TakenNow,
    [property: JsonPropertyName("allother")]
    IReadOnlyCollection<DrugToolFoodSubsectionDto>? AllOther);

/// <summary>
/// Supplement interaction entry for the drug tool.
/// </summary>
/// <param name="ActiveSubstanceId">Active substance identifier.</param>
/// <param name="ActiveSubstanceName">Active substance name.</param>
/// <param name="SupplementId">Supplement identifier.</param>
/// <param name="SupplementName">Supplement name.</param>
/// <param name="DefaultText">Default interaction text.</param>
/// <param name="StomachText">Optional stomach-related text.</param>
/// <param name="Severity">Severity label.</param>
public sealed record DrugToolSupplementInteractionDto(
    Guid ActiveSubstanceId,
    string ActiveSubstanceName,
    Guid SupplementId,
    string SupplementName,
    string DefaultText,
    string? StomachText,
    string Severity);

/// <summary>
/// Supplement interaction subsection grouped by subsection code.
/// </summary>
/// <param name="SubsectionCode">Subsection code identifier.</param>
/// <param name="RecordCount">Total interactions in the subsection.</param>
/// <param name="Interactions">Collection of interactions.</param>
public sealed record DrugToolSupplementSubsectionDto(
    string SubsectionCode,
    int RecordCount,
    IReadOnlyCollection<DrugToolSupplementInteractionDto> Interactions);

/// <summary>
/// Supplement interaction section containing taken-now and all-other subsections.
/// </summary>
/// <param name="Headline">Section headline.</param>
/// <param name="TakenNow">Interactions for supplements taken now.</param>
/// <param name="AllOther">Other interactions (serialized as allother).</param>
public sealed record DrugToolSupplementInteractionSectionDto(
    string Headline,
    IReadOnlyCollection<DrugToolSupplementSubsectionDto>? TakenNow,
    [property: JsonPropertyName("allother")]
    IReadOnlyCollection<DrugToolSupplementSubsectionDto>? AllOther);

/// <summary>
/// Full drug tool detail response sent to clients.
/// </summary>
/// <param name="DrugId">Identifier of the drug.</param>
/// <param name="CommercialName">Commercial name of the drug.</param>
/// <param name="TypeOfProduct">Product type.</param>
/// <param name="TypeOfDrug">Drug type enum value.</param>
/// <param name="SelectedDrugFormId">Identifier of the selected form.</param>
/// <param name="SelectedDrugFormSummary">Summary of the selected form.</param>
/// <param name="ActiveSubstances">Active substances involved.</param>
/// <param name="CurrentInteractionScope">Current interaction scope string.</param>
/// <param name="AllowedInteractionScopes">Allowed interaction scopes.</param>
/// <param name="AlcoholSection">Alcohol interaction section.</param>
/// <param name="DrugSection">Drug interaction section.</param>
/// <param name="FoodSection">Food interaction section.</param>
/// <param name="SupplementSection">Supplement interaction section.</param>
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
