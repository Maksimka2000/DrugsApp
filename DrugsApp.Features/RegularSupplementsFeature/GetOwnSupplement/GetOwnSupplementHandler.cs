using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.GetOwnSupplement;

/// <summary>
/// Handles retrieval of an own supplement.
/// </summary>
internal sealed class GetOwnSupplementHandler(
    IClientOwnSupplementRepository _clientOwnSupplementRepository)
    : IRequestHandler<GetOwnSupplementQuery, ErrorOr<GetOwnSupplementResponse>>
{
    public async Task<ErrorOr<GetOwnSupplementResponse>> Handle(
        GetOwnSupplementQuery query,
        CancellationToken cancellationToken)
    {
        Domain.RegularSupplementsFeature.Entities.ClientOwnSupplement? ownSupplement =
            await _clientOwnSupplementRepository.GetByIdForUserWithMembersAsync(
                query.UserId,
                query.Id,
                cancellationToken);
        if (ownSupplement is null)
        {
            return Error.NotFound(
                description: "Own supplement not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientOwnSupplement",
                    ["resourceId"] = query.Id.ToString(),
                });
        }

        return ownSupplement.MapToResponse();
    }
}
