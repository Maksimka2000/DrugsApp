using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.DeleteRegularSupplement;

/// <summary>
/// Handles permanent deletion of live regular supplement schedules.
/// </summary>
internal sealed class DeleteRegularSupplementHandler(
    IClientRegularSupplementRepository _clientRegularSupplementRepository)
    : IRequestHandler<DeleteRegularSupplementCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteRegularSupplementCommand command,
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
                description: "Regular supplement schedule is archived and cannot be deleted from the live endpoint.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        _clientRegularSupplementRepository.Remove(regularSupplement);

        return Result.Success;
    }
}
