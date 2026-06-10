using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Enums;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Features.RegularFoodFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularFoodFeature.UpdateRegularFoodSchedule;

/// <summary>
/// Handles updating regular food schedule dates and intake times.
/// </summary>
internal sealed class UpdateRegularFoodScheduleHandler(
    IClientRegularFoodRepository _clientRegularFoodRepository)
    : IRequestHandler<UpdateRegularFoodScheduleCommand, ErrorOr<UpdateRegularFoodScheduleResponse>>
{
    public async Task<ErrorOr<UpdateRegularFoodScheduleResponse>> Handle(
        UpdateRegularFoodScheduleCommand command,
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

        if (regularFood.Status == RegularFoodStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular food schedule is archived and cannot be updated.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        ErrorOr<Success> updated = regularFood.UpdateSchedule(
            command.Request.StartDate,
            command.Request.EndDate,
            command.Request.IntakeTimes,
            today);
        if (updated.IsError)
        {
            return updated.Errors;
        }

        return regularFood.MapToListItemDto().MapToResponse();
    }
}
