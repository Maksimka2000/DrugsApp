using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.CheckSupplementCustomNameAvailability;

internal sealed class CheckSupplementCustomNameAvailabilityHandler(IClientSupplementAllergyRepository _repository)
    : IRequestHandler<CheckSupplementCustomNameAvailabilityQuery, ErrorOr<NameAvailabilityResponse>>
{
    public async Task<ErrorOr<NameAvailabilityResponse>> Handle(
        CheckSupplementCustomNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalized = AllergySharedMappings.NormalizeForLookup(query.CustomSupplementName);
        if (await _repository.ExistsCustomNameNormalizedAsync(query.UserId, normalized, cancellationToken))
            return new NameAvailabilityResponse(false);
        if (await _repository.ExistsDatabaseDisplayNameNormalizedAsync(query.UserId, normalized, cancellationToken))
            return new NameAvailabilityResponse(false);
        return new NameAvailabilityResponse(true);
    }
}
