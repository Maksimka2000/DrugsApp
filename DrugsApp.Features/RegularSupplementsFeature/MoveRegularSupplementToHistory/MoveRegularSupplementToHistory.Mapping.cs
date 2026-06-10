using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.RegularSupplementsFeature.MoveRegularSupplementToHistory;

/// <summary>
/// Mapping helpers for move regular supplement to history use case.
/// </summary>
internal static class MoveRegularSupplementToHistoryMapping
{
    public static MoveRegularSupplementToHistoryResponse MapToResponse(
        this MoveRegularSupplementToHistorySummaryDto item) =>
        new(item);
}
