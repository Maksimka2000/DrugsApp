using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.UpdateRegularSupplement;

/// <summary>
/// Handles updating regular supplement schedules.
/// </summary>
internal sealed class UpdateRegularSupplementHandler(
    IClientRegularSupplementRepository _clientRegularSupplementRepository)
    : IRequestHandler<UpdateRegularSupplementCommand, ErrorOr<UpdateRegularSupplementResponse>>
{
    public async Task<ErrorOr<UpdateRegularSupplementResponse>> Handle(
        UpdateRegularSupplementCommand command,
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

        if (regularSupplement.Status == RegularSupplementStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular supplement schedule is archived and cannot be updated.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        ErrorOr<Success> updated = regularSupplement.UpdateSchedule(
            command.Request.StartDate,
            command.Request.EndDate,
            command.Request.IntakeTimes,
            today);
        if (updated.IsError)
        {
            return updated.Errors;
        }

        return regularSupplement.MapToDetailsDto().MapToResponse();
    }
}
