using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.CheckFoodCustomNameAvailability;

internal sealed class CheckFoodCustomNameAvailabilityHandler(IClientFoodAllergyRepository _repository)
    : IRequestHandler<CheckFoodCustomNameAvailabilityQuery, ErrorOr<NameAvailabilityResponse>>
{
    public async Task<ErrorOr<NameAvailabilityResponse>> Handle(
        CheckFoodCustomNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalized = AllergySharedMappings.NormalizeForLookup(query.CustomFoodName);
        if (await _repository.ExistsCustomNameNormalizedAsync(query.UserId, normalized, cancellationToken))
            return new NameAvailabilityResponse(false);
        if (await _repository.ExistsDatabaseDisplayNameNormalizedAsync(query.UserId, normalized, cancellationToken))
            return new NameAvailabilityResponse(false);
        return new NameAvailabilityResponse(true);
    }
}
