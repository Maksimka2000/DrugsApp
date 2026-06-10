using DrugsApp.Features.RegularFoodFeature.Shared;

namespace DrugsApp.Features.RegularFoodFeature.MoveRegularFoodToHistory;

/// <summary>
/// Mapping helpers for move regular food to history use case.
/// </summary>
internal static class MoveRegularFoodToHistoryMapping
{
    public static MoveRegularFoodToHistoryResponse MapToResponse(
        this MoveRegularFoodToHistorySummaryDto item) =>
        new(item);
}
