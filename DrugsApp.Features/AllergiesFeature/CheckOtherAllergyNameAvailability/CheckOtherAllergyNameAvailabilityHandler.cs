using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.CheckOtherAllergyNameAvailability;

internal sealed class CheckOtherAllergyNameAvailabilityHandler(IClientOtherAllergyRepository _repository)
    : IRequestHandler<CheckOtherAllergyNameAvailabilityQuery, ErrorOr<NameAvailabilityResponse>>
{
    public async Task<ErrorOr<NameAvailabilityResponse>> Handle(
        CheckOtherAllergyNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalized = AllergySharedMappings.NormalizeForLookup(query.AllergyName);
        bool exists = await _repository.ExistsNameNormalizedAsync(
            query.UserId, normalized, query.ExcludeId, cancellationToken);
        return new NameAvailabilityResponse(!exists);
    }
}
