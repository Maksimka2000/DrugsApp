using System;
using System.Collections.Generic;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugFeature.Shared;

/// <summary>
/// Data transfer object representing one searchable drug form candidate row.
/// </summary>
public sealed record SearchDrugFormDto(
    Guid DrugId,
    Guid? DrugFormId,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    IReadOnlyCollection<LookupDto> ActiveSubstances,
    string? Dosage,
    bool IsOral,
    IReadOnlyCollection<LookupDto> PharmaceuticalForms,
    string DisplayLine,
    bool IsSelectable);
