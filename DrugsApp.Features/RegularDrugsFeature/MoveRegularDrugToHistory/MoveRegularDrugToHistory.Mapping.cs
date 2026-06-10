using DrugsApp.Features.RegularDrugsFeature.Shared;

namespace DrugsApp.Features.RegularDrugsFeature.MoveRegularDrugToHistory;

/// <summary>
/// Mapping helpers for move regular drug to history use case.
/// </summary>
internal static class MoveRegularDrugToHistoryMapping
{
    public static MoveRegularDrugToHistoryResponse MapToResponse(this MoveRegularDrugToHistorySummaryDto item) =>
        new(item);
}
