using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.MoveRegularSupplementToHistory;

/// <summary>
/// Handles manual move-to-history for regular supplement schedules.
/// </summary>
internal sealed class MoveRegularSupplementToHistoryHandler(
    IClientRegularSupplementRepository _clientRegularSupplementRepository)
    : IRequestHandler<MoveRegularSupplementToHistoryCommand, ErrorOr<MoveRegularSupplementToHistoryResponse>>
{
    public async Task<ErrorOr<MoveRegularSupplementToHistoryResponse>> Handle(
        MoveRegularSupplementToHistoryCommand command,
        CancellationToken cancellationToken)
    {
        Domain.RegularSupplementsFeature.Entities.ClientRegularSupplement? regularSupplement =
            await _clientRegularSupplementRepository.GetTrackedByIdForUserAsync(
                command.UserId,
                command.Id,
                cancellationToken);
        if (regularSupplement is null)
        {
            return Error.NotFound(
                description: "Regular supplement schedule not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientRegularSupplement",
                    ["resourceId"] = command.Id.ToString(),
                });
        }

        ErrorOr<Success> moved = regularSupplement.MoveToHistory(
            RegularSupplementHistoryReason.ManualRemove,
            DateTime.UtcNow);
        if (moved.IsError)
        {
            return moved.Errors;
        }

        return regularSupplement.MapToMoveToHistorySummaryDto().MapToResponse();
    }
}
