using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Enums;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularFoodFeature.DeleteRegularFood;

/// <summary>
/// Handles permanent deletion of live regular food schedules.
/// </summary>
internal sealed class DeleteRegularFoodHandler(
    IClientRegularFoodRepository _clientRegularFoodRepository)
    : IRequestHandler<DeleteRegularFoodCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteRegularFoodCommand command,
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
                description: "Regular food schedule is archived and cannot be deleted from the live endpoint.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        _clientRegularFoodRepository.Remove(regularFood);

        return Result.Success;
    }
}
