using System;
using System.Collections.Generic;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Enums;

namespace DrugsApp.Features.RegularDrugsFeature.Shared;

/// <summary>
/// Shared projection for regular drug list, detail, and mutation responses.
/// </summary>
public sealed record RegularDrugListItemDto(
    Guid Id,
    Guid DrugId,
    Guid DrugFormId,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    IReadOnlyCollection<RegularDrugActiveSubstanceDto> ActiveSubstances,
    string FormulationName,
    string Dosage,
    RegularDrugStatus Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<string> IntakeTimes,
    int DailyIntakeCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Active substance lookup embedded in regular drug projections.
/// </summary>
public sealed record RegularDrugActiveSubstanceDto(
    Guid Id,
    string Name);

/// <summary>
/// History list item extending the live projection with archive metadata.
/// </summary>
public sealed record RegularDrugHistoryItemDto(
    Guid Id,
    Guid DrugId,
    Guid DrugFormId,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    IReadOnlyCollection<RegularDrugActiveSubstanceDto> ActiveSubstances,
    string FormulationName,
    string Dosage,
    RegularDrugStatus Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<string> IntakeTimes,
    int DailyIntakeCount,
    RegularDrugHistoryReason HistoryReason,
    DateTime ArchivedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Compact summary returned after manually moving a schedule to history.
/// </summary>
public sealed record MoveRegularDrugToHistorySummaryDto(
    Guid Id,
    string CommercialName,
    Guid DrugFormId,
    RegularDrugStatus Status,
    RegularDrugHistoryReason HistoryReason,
    DateTime ArchivedAtUtc);
