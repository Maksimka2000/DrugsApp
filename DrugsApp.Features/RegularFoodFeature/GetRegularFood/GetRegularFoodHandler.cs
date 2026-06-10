using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Enums;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularFoodFeature.GetRegularFood;

/// <summary>
/// Handles retrieval of a regular food schedule for update flows.
/// </summary>
internal sealed class GetRegularFoodHandler(
    IClientRegularFoodRepository _clientRegularFoodRepository)
    : IRequestHandler<GetRegularFoodQuery, ErrorOr<GetRegularFoodResponse>>
{
    public async Task<ErrorOr<GetRegularFoodResponse>> Handle(
        GetRegularFoodQuery query,
        CancellationToken cancellationToken)
    {
        Domain.RegularFoodFeature.Entities.ClientRegularFood? regularFood =
            await _clientRegularFoodRepository.GetByIdForUserAsync(
                query.UserId,
                query.Id,
                track: false,
                cancellationToken);
        if (regularFood is null)
        {
            return Error.NotFound(
                description: "Regular food schedule not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientRegularFood",
                    ["resourceId"] = query.Id.ToString(),
                });
        }

        if (regularFood.Status == RegularFoodStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular food schedule is archived and cannot be loaded for update.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        return regularFood.MapToResponse();
    }
}
