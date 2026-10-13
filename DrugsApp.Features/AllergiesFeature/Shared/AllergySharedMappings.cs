using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.Shared;

/// <summary>
/// Shared mapping helpers for allergy sources, answers, and DTO projections.
/// </summary>
public static class AllergySharedMappings
{
    /// <summary>
    /// Converts an allergy record source enum to its canonical token.
    /// </summary>
    /// <param name="source">Record source enum value.</param>
    /// <returns>Lowercase source token.</returns>
    public static string MapSource(AllergyRecordSource source) =>
        source switch
        {
            AllergyRecordSource.Database => "database",
            AllergyRecordSource.Custom => "custom",
            _ => "custom",
        };

    /// <summary>
    /// Converts an answer state enum to its canonical token.
    /// </summary>
    /// <param name="state">Nullable answer state.</param>
    /// <returns>Lowercase answer token or null when unknown.</returns>
    public static string? MapAnswerState(AllergyAnswerState? state) =>
        state switch
        {
            AllergyAnswerState.Yes => "yes",
            AllergyAnswerState.No => "no",
            AllergyAnswerState.DontKnow => "dont_know",
            _ => null,
        };

    /// <summary>
    /// Parses an answer state token to its enum representation.
    /// </summary>
    /// <param name="token">Canonical answer token.</param>
    /// <returns>Parsed answer state or null when invalid.</returns>
    public static AllergyAnswerState? MapAnswerStateFromToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return token.Trim().ToLowerInvariant() switch
        {
            "yes" => AllergyAnswerState.Yes,
            "no" => AllergyAnswerState.No,
            "dont_know" => AllergyAnswerState.DontKnow,
            _ => null,
        };
    }

    /// <summary>
    /// Parses a source token to its enum representation.
    /// </summary>
    /// <param name="token">Canonical source token.</param>
    /// <returns>Parsed record source or null when invalid.</returns>
    public static AllergyRecordSource? MapSourceFromToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return token.Trim().ToLowerInvariant() switch
        {
            "database" => AllergyRecordSource.Database,
            "custom" => AllergyRecordSource.Custom,
            _ => null,
        };
    }

    /// <summary>
    /// Maps an allergy summary aggregate to its response DTO.
    /// </summary>
    /// <param name="summary">Aggregate to map.</param>
    /// <returns>DTO with normalized answer tokens.</returns>
    public static AllergySummaryResponse MapToSummaryResponse(this ClientAllergySummary summary) =>
        new(
            Id: summary.Id,
            SevereReactionStatus: MapAnswerState(summary.SevereReactionStatus),
            CarriesAllergyMedication: summary.CarriesAllergyMedication,
            PollenDustMitesAllergyStatus: MapAnswerState(summary.PollenDustMitesAllergyStatus),
            InsectVenomAllergyStatus: MapAnswerState(summary.InsectVenomAllergyStatus),
            LatexAllergyStatus: MapAnswerState(summary.LatexAllergyStatus),
            CreatedAtUtc: summary.CreatedAtUtc,
            UpdatedAtUtc: summary.UpdatedAtUtc);

    /// <summary>
    /// Maps a drug allergy aggregate to its DTO with resolved display data.
    /// </summary>
    /// <param name="allergy">Drug allergy aggregate.</param>
    /// <returns>Drug allergy DTO.</returns>
    public static ClientDrugAllergyDto MapToDto(this ClientDrugAllergy allergy)
    {
        ClientDrugAllergyDrugFormSummaryDto? drugFormSummary = null;
        IReadOnlyCollection<LookupDto> activeSubstances = Array.Empty<LookupDto>();
        string displayName = ResolveDrugDisplayName(allergy);

        if (allergy.Source == AllergyRecordSource.Database &&
            allergy.DrugForm is not null &&
            allergy.DrugForm.Drug is not null)
        {
            DrugForm drugForm = allergy.DrugForm;
            Drug drug = drugForm.Drug;

            activeSubstances = drug.ActiveSubstances
                .OrderBy(activeSubstance => activeSubstance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(activeSubstance => activeSubstance.Id)
                .Select(activeSubstance => new LookupDto(activeSubstance.Id, activeSubstance.ActiveSubstanceName))
                .ToList();

            drugFormSummary = new ClientDrugAllergyDrugFormSummaryDto(
                drugForm.Id,
                drug.Id,
                drug.CommercialName,
                drug.TypeOfProduct,
                drug.TypeOfDrug,
                activeSubstances,
                BuildFormulationName(drugForm),
                drugForm.Dosage);

            displayName = BuildCatalogDisplayName(drug, drugForm);
        }
        else if (allergy.Source == AllergyRecordSource.Custom)
        {
            displayName = BuildCustomDisplayName(allergy);
        }

        return new ClientDrugAllergyDto(
            Id: allergy.Id,
            Source: MapSource(allergy.Source),
            DrugFormId: allergy.DrugFormId,
            CustomDrugName: allergy.CustomDrugName,
            CustomActiveSubstances: allergy.CustomActiveSubstances,
            CustomFormulationName: allergy.CustomFormulationName,
            CustomDosage: allergy.CustomDosage,
            DisplayName: displayName,
            Note: allergy.Note,
            ActiveSubstances: activeSubstances,
            CreatedAtUtc: allergy.CreatedAtUtc,
            UpdatedAtUtc: allergy.UpdatedAtUtc,
            DrugFormSummary: drugFormSummary);
    }

    /// <summary>
    /// Maps a collection of drug allergies to DTOs.
    /// </summary>
    /// <param name="allergies">Source collection.</param>
    /// <returns>List of DTOs.</returns>
    public static List<ClientDrugAllergyDto> MapToDrugDtoList(this IEnumerable<ClientDrugAllergy> allergies) =>
        allergies.Select(allergy => allergy.MapToDto()).ToList();

    /// <summary>
    /// Maps a supplement allergy aggregate to its DTO.
    /// </summary>
    /// <param name="allergy">Supplement allergy aggregate.</param>
    /// <returns>Supplement allergy DTO.</returns>
    public static ClientSupplementAllergyDto MapToDto(this ClientSupplementAllergy allergy) =>
        new(
            Id: allergy.Id,
            Source: MapSource(allergy.Source),
            SupplementId: allergy.SupplementId,
            CustomSupplementName: allergy.CustomSupplementName,
            DisplayName: ResolveSupplementDisplayName(allergy),
            Aliases: allergy.Supplement?.Aliases.ToList() ?? new List<string>(),
            Note: allergy.Note,
            CreatedAtUtc: allergy.CreatedAtUtc,
            UpdatedAtUtc: allergy.UpdatedAtUtc);

    /// <summary>
    /// Maps a collection of supplement allergies to DTOs.
    /// </summary>
    /// <param name="allergies">Source collection.</param>
    /// <returns>List of DTOs.</returns>
    public static List<ClientSupplementAllergyDto> MapToSupplementDtoList(
        this IEnumerable<ClientSupplementAllergy> allergies) =>
        allergies.Select(allergy => allergy.MapToDto()).ToList();

    /// <summary>
    /// Maps a food allergy aggregate to its DTO.
    /// </summary>
    /// <param name="allergy">Food allergy aggregate.</param>
    /// <returns>Food allergy DTO.</returns>
    public static ClientFoodAllergyDto MapToDto(this ClientFoodAllergy allergy) =>
        new(
            Id: allergy.Id,
            Source: MapSource(allergy.Source),
            FoodId: allergy.FoodId,
            CustomFoodName: allergy.CustomFoodName,
            DisplayName: ResolveFoodDisplayName(allergy),
            Aliases: allergy.Food?.Aliases.ToList() ?? new List<string>(),
            Note: allergy.Note,
            CreatedAtUtc: allergy.CreatedAtUtc,
            UpdatedAtUtc: allergy.UpdatedAtUtc);

    /// <summary>
    /// Maps a collection of food allergies to DTOs.
    /// </summary>
    /// <param name="allergies">Source collection.</param>
    /// <returns>List of DTOs.</returns>
    public static List<ClientFoodAllergyDto> MapToFoodDtoList(this IEnumerable<ClientFoodAllergy> allergies) =>
        allergies.Select(allergy => allergy.MapToDto()).ToList();

    /// <summary>
    /// Maps an "other" allergy aggregate to its DTO.
    /// </summary>
    /// <param name="allergy">Other allergy aggregate.</param>
    /// <returns>Other allergy DTO.</returns>
    public static ClientOtherAllergyDto MapToDto(this ClientOtherAllergy allergy) =>
        new(
            Id: allergy.Id,
            AllergyName: allergy.AllergyName,
            Note: allergy.Note,
            CreatedAtUtc: allergy.CreatedAtUtc,
            UpdatedAtUtc: allergy.UpdatedAtUtc);

    /// <summary>
    /// Maps a collection of "other" allergies to DTOs.
    /// </summary>
    /// <param name="allergies">Source collection.</param>
    /// <returns>List of DTOs.</returns>
    public static List<ClientOtherAllergyDto> MapToOtherDtoList(this IEnumerable<ClientOtherAllergy> allergies) =>
        allergies.Select(allergy => allergy.MapToDto()).ToList();

    /// <summary>
    /// Normalizes a string for case-insensitive lookup comparisons.
    /// </summary>
    /// <param name="value">Value to normalize.</param>
    /// <returns>Lowercased, collapsed string.</returns>
    public static string NormalizeForLookup(string value)
    {
        string trimmed = value.Trim();
        string collapsed = string.Join(
            ' ',
            trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return collapsed.ToLowerInvariant();
    }

    private static string ResolveDrugDisplayName(ClientDrugAllergy allergy)
    {
        if (allergy.Source == AllergyRecordSource.Custom)
        {
            return BuildCustomDisplayName(allergy);
        }

        if (allergy.DrugForm?.Drug is not null)
        {
            return BuildCatalogDisplayName(allergy.DrugForm.Drug, allergy.DrugForm);
        }

        return allergy.CustomDrugName ?? string.Empty;
    }

    private static string BuildCustomDisplayName(ClientDrugAllergy allergy)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(allergy.CustomDrugName))
        {
            parts.Add(allergy.CustomDrugName);
        }

        if (!string.IsNullOrWhiteSpace(allergy.CustomActiveSubstances))
        {
            parts.Add(allergy.CustomActiveSubstances);
        }

        if (!string.IsNullOrWhiteSpace(allergy.CustomFormulationName))
        {
            parts.Add(allergy.CustomFormulationName);
        }

        if (!string.IsNullOrWhiteSpace(allergy.CustomDosage))
        {
            parts.Add(allergy.CustomDosage);
        }

        return string.Join(" | ", parts);
    }

    private static string BuildCatalogDisplayName(Drug drug, DrugForm drugForm)
    {
        var parts = new List<string> { drug.CommercialName };

        string activeSubstanceSegment = string.Join(
            ", ",
            drug.ActiveSubstances
                .OrderBy(substance => substance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(substance => substance.Id)
                .Select(substance => substance.ActiveSubstanceName));

        if (!string.IsNullOrWhiteSpace(activeSubstanceSegment))
        {
            parts.Add(activeSubstanceSegment);
        }

        string formulationName = BuildFormulationName(drugForm);
        if (!string.IsNullOrWhiteSpace(formulationName))
        {
            parts.Add(formulationName);
        }

        if (!string.IsNullOrWhiteSpace(drugForm.Dosage))
        {
            parts.Add(drugForm.Dosage);
        }

        if (drug.TypeOfDrug is DrugType.Special or DrugType.Ophthalmic)
        {
            parts.Add(drug.TypeOfDrug.ToString());
        }

        if (drug.TypeOfProduct.Equals("Weterynaryjny", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add(drug.TypeOfProduct);
        }

        return string.Join(" | ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string BuildFormulationName(DrugForm drugForm) =>
        string.Join(
            ", ",
            drugForm.PharmaceuticalForms
                .Select(form => form.FormulationName)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));

    private static string ResolveSupplementDisplayName(ClientSupplementAllergy allergy)
    {
        if (allergy.Source == AllergyRecordSource.Custom && allergy.CustomSupplementName is not null)
        {
            return allergy.CustomSupplementName;
        }

        if (allergy.Supplement is not null)
        {
            return allergy.Supplement.Name;
        }

        return string.Empty;
    }

    private static string ResolveFoodDisplayName(ClientFoodAllergy allergy)
    {
        if (allergy.Source == AllergyRecordSource.Custom && allergy.CustomFoodName is not null)
        {
            return allergy.CustomFoodName;
        }

        if (allergy.Food is not null)
        {
            return allergy.Food.Name;
        }

        return string.Empty;
    }
}
