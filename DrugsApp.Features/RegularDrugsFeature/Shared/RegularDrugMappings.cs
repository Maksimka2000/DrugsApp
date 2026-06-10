using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Enums;

namespace DrugsApp.Features.RegularDrugsFeature.Shared;

/// <summary>
/// Mapping helpers for regular drug aggregates and DTO representations.
/// </summary>
internal static class RegularDrugMappings
{
    public static RegularDrugListItemDto MapToListItemDto(this ClientRegularDrug regularDrug) => new(
        Id: regularDrug.Id,
        DrugId: regularDrug.DrugId,
        DrugFormId: regularDrug.DrugFormId,
        CommercialName: regularDrug.Drug.CommercialName,
        TypeOfProduct: regularDrug.Drug.TypeOfProduct,
        TypeOfDrug: regularDrug.Drug.TypeOfDrug,
        ActiveSubstances: MapActiveSubstances(regularDrug.Drug),
        FormulationName: BuildFormulationName(regularDrug.DrugForm),
        Dosage: regularDrug.DrugForm.Dosage,
        Status: regularDrug.Status,
        StartDate: regularDrug.StartDate,
        EndDate: regularDrug.EndDate,
        IntakeTimes: MapIntakeTimes(regularDrug),
        DailyIntakeCount: regularDrug.IntakeTimes.Count,
        CreatedAtUtc: regularDrug.CreatedAtUtc,
        UpdatedAtUtc: regularDrug.UpdatedAtUtc);

    public static RegularDrugHistoryItemDto MapToHistoryItemDto(this ClientRegularDrug regularDrug) => new(
        Id: regularDrug.Id,
        DrugId: regularDrug.DrugId,
        DrugFormId: regularDrug.DrugFormId,
        CommercialName: regularDrug.Drug.CommercialName,
        TypeOfProduct: regularDrug.Drug.TypeOfProduct,
        TypeOfDrug: regularDrug.Drug.TypeOfDrug,
        ActiveSubstances: MapActiveSubstances(regularDrug.Drug),
        FormulationName: BuildFormulationName(regularDrug.DrugForm),
        Dosage: regularDrug.DrugForm.Dosage,
        Status: regularDrug.Status,
        StartDate: regularDrug.StartDate,
        EndDate: regularDrug.EndDate,
        IntakeTimes: MapIntakeTimes(regularDrug),
        DailyIntakeCount: regularDrug.IntakeTimes.Count,
        HistoryReason: regularDrug.HistoryReason ?? RegularDrugHistoryReason.ManualRemove,
        ArchivedAtUtc: regularDrug.ArchivedAtUtc ?? regularDrug.UpdatedAtUtc ?? regularDrug.CreatedAtUtc,
        CreatedAtUtc: regularDrug.CreatedAtUtc,
        UpdatedAtUtc: regularDrug.UpdatedAtUtc);

    public static MoveRegularDrugToHistorySummaryDto MapToMoveToHistorySummaryDto(this ClientRegularDrug regularDrug) =>
        new(
            Id: regularDrug.Id,
            CommercialName: regularDrug.Drug.CommercialName,
            DrugFormId: regularDrug.DrugFormId,
            Status: regularDrug.Status,
            HistoryReason: regularDrug.HistoryReason ?? RegularDrugHistoryReason.ManualRemove,
            ArchivedAtUtc: regularDrug.ArchivedAtUtc ?? DateTime.UtcNow);

    public static string BuildFormulationName(DrugForm drugForm) =>
        string.Join(
            ", ",
            drugForm.PharmaceuticalForms
                .Select(form => form.FormulationName)
                .OrderBy(name => name));

    public static string BuildNotificationSummary(ClientRegularDrug regularDrug)
    {
        string formulationName = BuildFormulationName(regularDrug.DrugForm);
        return $"{regularDrug.Drug.CommercialName} ({formulationName}, {regularDrug.DrugForm.Dosage}) has ended.";
    }

    private static IReadOnlyCollection<RegularDrugActiveSubstanceDto> MapActiveSubstances(Drug drug) =>
        drug.ActiveSubstances
            .Select(activeSubstance => new RegularDrugActiveSubstanceDto(
                activeSubstance.Id,
                activeSubstance.ActiveSubstanceName))
            .OrderBy(activeSubstance => activeSubstance.Name)
            .ToList();

    private static IReadOnlyCollection<string> MapIntakeTimes(ClientRegularDrug regularDrug) =>
        regularDrug.IntakeTimes
            .Select(intakeTime => intakeTime.IntakeTime.ToString("HH:mm", CultureInfo.InvariantCulture))
            .OrderBy(time => time, StringComparer.Ordinal)
            .ToList();
}
