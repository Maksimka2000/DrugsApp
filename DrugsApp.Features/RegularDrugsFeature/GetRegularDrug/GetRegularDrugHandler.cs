using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularDrugsFeature.GetRegularDrug;

/// <summary>
/// Handles retrieval of a regular drug schedule for update flows.
/// </summary>
internal sealed class GetRegularDrugHandler(
    IClientRegularDrugRepository _clientRegularDrugRepository)
    : IRequestHandler<GetRegularDrugQuery, ErrorOr<GetRegularDrugResponse>>
{
    public async Task<ErrorOr<GetRegularDrugResponse>> Handle(
        GetRegularDrugQuery query,
        CancellationToken cancellationToken)
    {
        Domain.RegularDrugsFeature.Entities.ClientRegularDrug? regularDrug =
            await _clientRegularDrugRepository.GetByIdForUserAsync(
                query.UserId,
                query.Id,
                track: false,
                cancellationToken);
        if (regularDrug is null)
        {
            return Error.NotFound(
                description: "Regular drug schedule not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientRegularDrug",
                    ["resourceId"] = query.Id.ToString(),
                });
        }

        if (regularDrug.Status == RegularDrugStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular drug schedule is archived and cannot be loaded for update.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        return regularDrug.MapToResponse();
    }
}
