using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.Shared;

public static class AllergySharedMappings
{
    public static string MapSource(AllergyRecordSource source) =>
        source switch
        {
            AllergyRecordSource.Database => "database",
            AllergyRecordSource.Custom => "custom",
            _ => "custom",
        };

    public static string? MapAnswerState(AllergyAnswerState? state) =>
        state switch
        {
            AllergyAnswerState.Yes => "yes",
            AllergyAnswerState.No => "no",
            AllergyAnswerState.DontKnow => "dont_know",
            _ => null,
        };

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

    public static ClientDrugAllergyDto MapToDto(this ClientDrugAllergy allergy)
    {
        string displayName = ResolveDrugDisplayName(allergy);
        IReadOnlyCollection<LookupDto> activeSubstances = allergy.Source == AllergyRecordSource.Database &&
                                                            allergy.Drug is not null
            ? allergy.Drug.ActiveSubstances
                .Select(activeSubstance => new LookupDto(activeSubstance.Id, activeSubstance.ActiveSubstanceName))
                .ToList()
            : Array.Empty<LookupDto>();

        return new ClientDrugAllergyDto(
            Id: allergy.Id,
            Source: MapSource(allergy.Source),
            DrugId: allergy.DrugId,
            CustomDrugName: allergy.CustomDrugName,
            DisplayName: displayName,
            Note: allergy.Note,
            ActiveSubstances: activeSubstances,
            CreatedAtUtc: allergy.CreatedAtUtc,
            UpdatedAtUtc: allergy.UpdatedAtUtc);
    }

    public static ClientDrugAllergyDto MapToDto(this ClientDrugAllergy allergy, Drug drug)
    {
        IReadOnlyCollection<LookupDto> activeSubstances = drug.ActiveSubstances
            .Select(activeSubstance => new LookupDto(activeSubstance.Id, activeSubstance.ActiveSubstanceName))
            .ToList();

        return new ClientDrugAllergyDto(
            Id: allergy.Id,
            Source: MapSource(allergy.Source),
            DrugId: allergy.DrugId,
            CustomDrugName: allergy.CustomDrugName,
            DisplayName: drug.CommercialName,
            Note: allergy.Note,
            ActiveSubstances: activeSubstances,
            CreatedAtUtc: allergy.CreatedAtUtc,
            UpdatedAtUtc: allergy.UpdatedAtUtc);
    }

    public static List<ClientDrugAllergyDto> MapToDrugDtoList(this IEnumerable<ClientDrugAllergy> allergies) =>
        allergies.Select(allergy => allergy.MapToDto()).ToList();

    public static ClientSupplementAllergyDto MapToDto(this ClientSupplementAllergy allergy) =>
        new(
            Id: allergy.Id,
            Source: MapSource(allergy.Source),
            SupplementId: allergy.SupplementId,
            CustomSupplementName: allergy.CustomSupplementName,
            DisplayName: ResolveSupplementDisplayName(allergy),
            Note: allergy.Note,
            CreatedAtUtc: allergy.CreatedAtUtc,
            UpdatedAtUtc: allergy.UpdatedAtUtc);

    public static List<ClientSupplementAllergyDto> MapToSupplementDtoList(
        this IEnumerable<ClientSupplementAllergy> allergies) =>
        allergies.Select(allergy => allergy.MapToDto()).ToList();

    public static ClientFoodAllergyDto MapToDto(this ClientFoodAllergy allergy) =>
        new(
            Id: allergy.Id,
            Source: MapSource(allergy.Source),
            FoodId: allergy.FoodId,
            CustomFoodName: allergy.CustomFoodName,
            DisplayName: ResolveFoodDisplayName(allergy),
            Note: allergy.Note,
            CreatedAtUtc: allergy.CreatedAtUtc,
            UpdatedAtUtc: allergy.UpdatedAtUtc);

    public static List<ClientFoodAllergyDto> MapToFoodDtoList(this IEnumerable<ClientFoodAllergy> allergies) =>
        allergies.Select(allergy => allergy.MapToDto()).ToList();

    public static ClientOtherAllergyDto MapToDto(this ClientOtherAllergy allergy) =>
        new(
            Id: allergy.Id,
            AllergyName: allergy.AllergyName,
            Note: allergy.Note,
            CreatedAtUtc: allergy.CreatedAtUtc,
            UpdatedAtUtc: allergy.UpdatedAtUtc);

    public static List<ClientOtherAllergyDto> MapToOtherDtoList(this IEnumerable<ClientOtherAllergy> allergies) =>
        allergies.Select(allergy => allergy.MapToDto()).ToList();

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
        if (allergy.Source == AllergyRecordSource.Custom && allergy.CustomDrugName is not null)
        {
            return allergy.CustomDrugName;
        }

        if (allergy.Drug is not null)
        {
            return allergy.Drug.CommercialName;
        }

        return string.Empty;
    }

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
