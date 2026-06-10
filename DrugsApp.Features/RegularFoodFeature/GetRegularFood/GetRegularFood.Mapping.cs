using DrugsApp.Features.RegularFoodFeature.Shared;

namespace DrugsApp.Features.RegularFoodFeature.GetRegularFood;

/// <summary>
/// Mapping helpers for get regular food use case.
/// </summary>
internal static class GetRegularFoodMapping
{
    public static GetRegularFoodResponse MapToResponse(this Domain.RegularFoodFeature.Entities.ClientRegularFood regularFood) =>
        new(regularFood.MapToListItemDto());
}
