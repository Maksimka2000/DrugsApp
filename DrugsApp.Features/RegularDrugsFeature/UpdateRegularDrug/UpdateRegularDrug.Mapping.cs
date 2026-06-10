using System;
using DrugsApp.Features.RegularDrugsFeature.Shared;

namespace DrugsApp.Features.RegularDrugsFeature.UpdateRegularDrug;

/// <summary>
/// Mapping helpers for update regular drug use case.
/// </summary>
internal static class UpdateRegularDrugMapping
{
    public static UpdateRegularDrugCommand MapToCommand(
        this UpdateRegularDrugRequest request,
        Guid userId,
        Guid id) =>
        new(userId, id, request);

    public static UpdateRegularDrugResponse MapToResponse(this RegularDrugListItemDto item) =>
        new(item);
}
