using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;

namespace DrugsApp.Features.DrugFeature.UpdateDrug;

internal static class UpdateDrugMapping
{
    public static UpdateDrugCommand MapToCommand(this UpdateDrugRequest request, Guid drugId) => new(
        DrugId: drugId,
        Request: request);

    public static UpdatedDrugResponse MapToResponse(this Drug drug) => new(
        Id: drug.Id,
        CommercialName: drug.CommercialName,
        TypeOfProduct: drug.TypeOfProduct,
        TypeOfDrug: drug.TypeOfDrug,
        ActiveSubstances: drug.ActiveSubstances
            .OrderBy(activeSubstance => activeSubstance.ActiveSubstanceName)
            .Select(activeSubstance => new LookupDto(Id: activeSubstance.Id, Name: activeSubstance.ActiveSubstanceName))
            .ToList());

    // public static IReadOnlyCollection<Guid> ToIdSet(this IEnumerable<Guid> ids)
    //     => ids.Distinct().ToList();
}

