using System;
using System.Collections.Generic;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugFeature.Shared;

/// <summary>
/// Data transfer object representing a drug with its composition and forms.
/// </summary>
public sealed record DrugDto(
    Guid Id,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    int DrugFormsCount,
    IReadOnlyCollection<LookupDto> ActiveSubstances,
    IReadOnlyCollection<DrugFormDto> DrugForms);

