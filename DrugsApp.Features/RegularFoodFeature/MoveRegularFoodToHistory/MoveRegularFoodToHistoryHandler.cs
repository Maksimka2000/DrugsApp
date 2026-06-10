using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Features.RegularFoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularFoodFeature.MoveRegularFoodToHistory;

/// <summary>
/// Handles manual archive of live regular food schedules.
/// </summary>
internal sealed class MoveRegularFoodToHistoryHandler(
    IClientRegularFoodRepository _clientRegularFoodRepository)
    : IRequestHandler<MoveRegularFoodToHistoryCommand, ErrorOr<MoveRegularFoodToHistoryResponse>>
{
    public async Task<ErrorOr<MoveRegularFoodToHistoryResponse>> Handle(
        MoveRegularFoodToHistoryCommand command,
        CancellationToken cancellationToken)
    {
        Domain.RegularFoodFeature.Entities.ClientRegularFood? regularFood =
            await _clientRegularFoodRepository.GetByIdForUserAsync(
                command.UserId,
                command.Id,
                track: true,
                cancellationToken);
        if (regularFood is null)
        {
            return Error.NotFound(
                description: "Regular food schedule not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientRegularFood",
                    ["resourceId"] = command.Id.ToString(),
                });
        }

        ErrorOr<Success> moved = regularFood.MoveToHistoryManual(DateTime.UtcNow);
        if (moved.IsError)
        {
            return moved.Errors;
        }

        return regularFood.MapToMoveToHistorySummaryDto().MapToResponse();
    }
}
