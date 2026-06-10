using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.GetRegularSupplement;

/// <summary>
/// Handles retrieval of a regular supplement schedule for update flows.
/// </summary>
internal sealed class GetRegularSupplementHandler(
    IClientRegularSupplementRepository _clientRegularSupplementRepository)
    : IRequestHandler<GetRegularSupplementQuery, ErrorOr<GetRegularSupplementResponse>>
{
    public async Task<ErrorOr<GetRegularSupplementResponse>> Handle(
        GetRegularSupplementQuery query,
        CancellationToken cancellationToken)
    {
        Domain.RegularSupplementsFeature.Entities.ClientRegularSupplement? regularSupplement =
            await _clientRegularSupplementRepository.GetByIdForUserAsync(
                query.UserId,
                query.Id,
                cancellationToken);
        if (regularSupplement is null)
        {
            return Error.NotFound(
                description: "Regular supplement schedule not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientRegularSupplement",
                    ["resourceId"] = query.Id.ToString(),
                });
        }

        if (regularSupplement.Status == RegularSupplementStatus.Expired)
        {
            return Error.Conflict(
                description: "Regular supplement schedule is archived and cannot be loaded for update.",
                metadata: new()
                {
                    ["field"] = "status",
                    ["reason"] = "archived",
                });
        }

        return regularSupplement.MapToResponse();
    }
}
