using DrugsApp.Features.RegularDrugsFeature.Shared;

namespace DrugsApp.Features.RegularDrugsFeature.GetRegularDrug;

/// <summary>
/// Mapping helpers for get regular drug use case.
/// </summary>
internal static class GetRegularDrugMapping
{
    public static GetRegularDrugResponse MapToResponse(
        this Domain.RegularDrugsFeature.Entities.ClientRegularDrug regularDrug) =>
        new(regularDrug.MapToListItemDto());
}
