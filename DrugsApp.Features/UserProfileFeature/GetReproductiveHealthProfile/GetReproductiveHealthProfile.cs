using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.UserProfileFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.UserProfileFeature.GetReproductiveHealthProfile;

public sealed record GetReproductiveHealthProfileQuery(Guid UserId)
    : IRequest<ErrorOr<ReproductiveHealthProfileResponse>>;

internal sealed class GetReproductiveHealthProfileHandler(
    IClientReproductiveHealthProfileRepository reproductiveHealthProfileRepository)
    : IRequestHandler<GetReproductiveHealthProfileQuery, ErrorOr<ReproductiveHealthProfileResponse>>
{
    public async Task<ErrorOr<ReproductiveHealthProfileResponse>> Handle(
        GetReproductiveHealthProfileQuery query,
        CancellationToken cancellationToken)
    {
        ClientReproductiveHealthProfile? profile = await reproductiveHealthProfileRepository
            .GetTrackedByUserIdAsync(query.UserId, cancellationToken);

        if (profile is null)
        {
            return Error.NotFound(
                description: "Reproductive health profile not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientReproductiveHealthProfile",
                    ["resourceId"] = query.UserId.ToString(),
                });
        }

        return ReproductiveHealthProfileResponse.From(profile);
    }
}
