using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularDrugsFeature.DeleteRegularDrug;

/// <summary>
/// Handles permanent deletion of live regular drug schedules.
/// </summary>
internal sealed class DeleteRegularDrugHandler(
    IClientRegularDrugRepository _clientRegularDrugRepository)
    : IRequestHandler<DeleteRegularDrugCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        DeleteRegularDrugCommand command,
        CancellationToken cancellationToken)
    {
        Domain.RegularDrugsFeature.Entities.ClientRegularDrug? regularDrug =
            await _clientRegularDrugRepository.GetByIdForUserAsync(
                command.UserId,
                command.Id,
                track: true,
                cancellationToken);
        if (regularDrug is null)
        {
            return Error.NotFound(
                description: "Regular drug schedule not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientRegularDrug",
                    ["resourceId"] = command.Id.ToString(),
                });
        }

        if (regularDrug.Status == RegularDrugStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular drug schedule is archived and cannot be deleted from the live endpoint.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        _clientRegularDrugRepository.Remove(regularDrug);

        return Result.Success;
    }
}
