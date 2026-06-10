using System;
using DrugsApp.Features.RegularFoodFeature.Shared;

namespace DrugsApp.Features.RegularFoodFeature.CreateRegularFood;

/// <summary>
/// Mapping helpers for create regular food use case.
/// </summary>
internal static class CreateRegularFoodMapping
{
    public static CreateRegularFoodCommand MapToCommand(this CreateRegularFoodRequest request, Guid userId) =>
        new(userId, request);

    public static CreateRegularFoodResponse MapToResponse(this RegularFoodListItemDto item) =>
        new(item);
}
