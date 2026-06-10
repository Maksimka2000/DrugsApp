using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Features.RegularDrugsFeature.ApplicationServices;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularDrugsFeature.MoveRegularDrugToHistory;

/// <summary>
/// Handles manual move-to-history for regular drug schedules.
/// </summary>
internal sealed class MoveRegularDrugToHistoryHandler(
    IRegularDrugsApplicationService _regularDrugsApplicationService)
    : IRequestHandler<MoveRegularDrugToHistoryCommand, ErrorOr<MoveRegularDrugToHistoryResponse>>
{
    public async Task<ErrorOr<MoveRegularDrugToHistoryResponse>> Handle(
        MoveRegularDrugToHistoryCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<MoveRegularDrugToHistorySummaryDto> moved = await _regularDrugsApplicationService.MoveToHistoryAsync(
            command.UserId,
            command.Id,
            RegularDrugHistoryReason.ManualRemove,
            cancellationToken);
        if (moved.IsError)
        {
            return moved.Errors;
        }

        return moved.Value.MapToResponse();
    }
}
