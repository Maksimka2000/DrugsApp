using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Enums;
using DrugsApp.Features.DrugFeature.Shared;

namespace DrugsApp.Features.DrugProblemsFeature.Shared;

/// <summary>
/// List item DTO for a drug problem entry.
/// </summary>
public sealed record DrugProblemListItemDto(
    Guid Id,
    string Source,
    Guid? DrugId,
    string? CustomDrugName,
    string DisplayName,
    string ProblemNote,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DrugProblemDrugSummaryDto? DrugSummary);

/// <summary>
/// Full details DTO for a drug problem entry.
/// </summary>
public sealed record DrugProblemDetailsDto(DrugProblemListItemDto Item);

/// <summary>
/// Compact drug summary used when a drug problem references a catalog drug.
/// </summary>
public sealed record DrugProblemDrugSummaryDto(
    Guid Id,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    IReadOnlyCollection<LookupDto> ActiveSubstances);

public static class DrugProblemMappings
{
    public static DrugProblemListItemDto MapToListItemDto(this DrugProblem drugProblem)
    {
        string source = MapSource(drugProblem.Source);
        string displayName = ResolveDisplayName(drugProblem);

        DrugProblemDrugSummaryDto? drugSummary = null;
        if (drugProblem.Source == DrugProblemSource.Database && drugProblem.Drug is not null)
        {
            IReadOnlyCollection<LookupDto> activeSubstances = drugProblem.Drug.ActiveSubstances
                .Select(activeSubstance => new LookupDto(activeSubstance.Id, activeSubstance.ActiveSubstanceName))
                .ToList();

            drugSummary = new DrugProblemDrugSummaryDto(
                drugProblem.Drug.Id,
                drugProblem.Drug.CommercialName,
                drugProblem.Drug.TypeOfProduct,
                drugProblem.Drug.TypeOfDrug,
                activeSubstances);
        }

        return new DrugProblemListItemDto(
            drugProblem.Id,
            source,
            drugProblem.DrugId,
            drugProblem.CustomDrugName,
            displayName,
            drugProblem.ProblemNote,
            drugProblem.CreatedAtUtc,
            drugProblem.UpdatedAtUtc,
            drugSummary);
    }

    public static DrugProblemDetailsDto MapToDetailsDto(this DrugProblem drugProblem) =>
        new(drugProblem.MapToListItemDto());

    private static string ResolveDisplayName(DrugProblem drugProblem)
    {
        if (drugProblem.Source == DrugProblemSource.Custom && drugProblem.CustomDrugName is not null)
        {
            return drugProblem.CustomDrugName;
        }

        if (drugProblem.Drug is not null)
        {
            return drugProblem.Drug.CommercialName;
        }

        return drugProblem.CustomDrugName ?? string.Empty;
    }

    private static string MapSource(DrugProblemSource source) =>
        source == DrugProblemSource.Database ? "database" : "custom";
}
