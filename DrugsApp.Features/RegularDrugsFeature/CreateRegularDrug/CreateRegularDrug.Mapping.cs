using System;
using DrugsApp.Features.RegularDrugsFeature.Shared;

namespace DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;

/// <summary>
/// Mapping helpers for create regular drug use case.
/// </summary>
internal static class CreateRegularDrugMapping
{
    public static CreateRegularDrugCommand MapToCommand(this CreateRegularDrugRequest request, Guid userId) =>
        new(userId, request);

    public static CreateRegularDrugResponse MapToResponse(this RegularDrugListItemDto item) =>
        new(item);
}
