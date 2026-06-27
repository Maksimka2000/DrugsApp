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
/// <param name="Id">Drug problem identifier.</param>
/// <param name="Source">Indicates whether the problem comes from database or custom source.</param>
/// <param name="DrugFormId">Catalog drug form identifier when applicable.</param>
/// <param name="CustomDrugName">Custom drug name when user entered.</param>
/// <param name="CustomActiveSubstances">Custom active substances description.</param>
/// <param name="CustomFormulationName">Custom pharmaceutical form description.</param>
/// <param name="CustomDosage">Custom dosage text.</param>
/// <param name="DisplayName">Human readable display line.</param>
/// <param name="ProblemNote">User-provided problem note.</param>
/// <param name="CreatedAtUtc">Creation timestamp.</param>
/// <param name="UpdatedAtUtc">Last update timestamp.</param>
/// <param name="DrugFormSummary">Catalog drug form summary when present.</param>
public sealed record DrugProblemListItemDto(
    Guid Id,
    string Source,
    Guid? DrugFormId,
    string? CustomDrugName,
    string? CustomActiveSubstances,
    string? CustomFormulationName,
    string? CustomDosage,
    string DisplayName,
    string ProblemNote,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DrugProblemDrugFormSummaryDto? DrugFormSummary);

/// <summary>
/// Full details DTO for a drug problem entry.
/// </summary>
/// <param name="Item">Wrapped list item DTO.</param>
public sealed record DrugProblemDetailsDto(DrugProblemListItemDto Item);

/// <summary>
/// Compact drug form summary used when a drug problem references a catalog drug form.
/// </summary>
/// <param name="DrugFormId">Identifier of the drug form.</param>
/// <param name="DrugId">Identifier of the parent drug.</param>
/// <param name="CommercialName">Drug commercial name.</param>
/// <param name="TypeOfProduct">Type of product descriptor.</param>
/// <param name="TypeOfDrug">Drug type enumeration value.</param>
/// <param name="ActiveSubstances">Active substances involved.</param>
/// <param name="FormulationName">Name of the pharmaceutical formulation.</param>
/// <param name="Dosage">Dosage text if available.</param>
public sealed record DrugProblemDrugFormSummaryDto(
    Guid DrugFormId,
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    IReadOnlyCollection<LookupDto> ActiveSubstances,
    string FormulationName,
    string? Dosage);

/// <summary>
/// Mapping helpers for transforming drug problems to DTOs.
/// </summary>
public static class DrugProblemMappings
{
    /// <summary>
    /// Projects a drug problem entity to a list item DTO, resolving source and display name.
    /// </summary>
    /// <param name="drugProblem">Drug problem entity to map.</param>
    /// <returns>DTO representing the list item.</returns>
    public static DrugProblemListItemDto MapToListItemDto(this DrugProblem drugProblem)
    {
        string source = MapSource(drugProblem.Source);
        string displayName = ResolveDisplayName(drugProblem);

        DrugProblemDrugFormSummaryDto? drugFormSummary = null;
        if (drugProblem.Source == DrugProblemSource.Database &&
            drugProblem.DrugForm is not null &&
            drugProblem.DrugForm.Drug is not null)
        {
            DrugForm drugForm = drugProblem.DrugForm;
            Drug drug = drugForm.Drug;

            IReadOnlyCollection<LookupDto> activeSubstances = drug.ActiveSubstances
                .OrderBy(activeSubstance => activeSubstance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(activeSubstance => activeSubstance.Id)
                .Select(activeSubstance => new LookupDto(activeSubstance.Id, activeSubstance.ActiveSubstanceName))
                .ToList();

            drugFormSummary = new DrugProblemDrugFormSummaryDto(
                drugForm.Id,
                drug.Id,
                drug.CommercialName,
                drug.TypeOfProduct,
                drug.TypeOfDrug,
                activeSubstances,
                BuildFormulationName(drugForm),
                drugForm.Dosage);
        }

        return new DrugProblemListItemDto(
            drugProblem.Id,
            source,
            drugProblem.DrugFormId,
            drugProblem.CustomDrugName,
            drugProblem.CustomActiveSubstances,
            drugProblem.CustomFormulationName,
            drugProblem.CustomDosage,
            displayName,
            drugProblem.ProblemNote,
            drugProblem.CreatedAtUtc,
            drugProblem.UpdatedAtUtc,
            drugFormSummary);
    }

    /// <summary>
    /// Projects a drug problem entity to a details DTO.
    /// </summary>
    /// <param name="drugProblem">Drug problem entity to map.</param>
    /// <returns>DTO with full details.</returns>
    public static DrugProblemDetailsDto MapToDetailsDto(this DrugProblem drugProblem) =>
        new(drugProblem.MapToListItemDto());

    private static string ResolveDisplayName(DrugProblem drugProblem)
    {
        if (drugProblem.Source == DrugProblemSource.Custom && drugProblem.CustomDrugName is not null)
        {
            var parts = new List<string> { drugProblem.CustomDrugName };

            if (!string.IsNullOrWhiteSpace(drugProblem.CustomActiveSubstances))
            {
                parts.Add(drugProblem.CustomActiveSubstances);
            }

            if (!string.IsNullOrWhiteSpace(drugProblem.CustomFormulationName))
            {
                parts.Add(drugProblem.CustomFormulationName);
            }

            if (!string.IsNullOrWhiteSpace(drugProblem.CustomDosage))
            {
                parts.Add(drugProblem.CustomDosage);
            }

            return string.Join(" | ", parts);
        }

        if (drugProblem.DrugForm?.Drug is not null)
        {
            return BuildCatalogDisplayName(drugProblem.DrugForm.Drug, drugProblem.DrugForm);
        }

        return drugProblem.CustomDrugName ?? string.Empty;
    }

    internal static string BuildCatalogDisplayName(Drug drug, DrugForm drugForm)
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

    private static string MapSource(DrugProblemSource source) =>
        source == DrugProblemSource.Database ? "database" : "custom";
}
