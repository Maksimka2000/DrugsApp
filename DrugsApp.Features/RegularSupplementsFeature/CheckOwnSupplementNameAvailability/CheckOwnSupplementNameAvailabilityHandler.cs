using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.RegularSupplementsFeature.CheckOwnSupplementNameAvailability;

/// <summary>
/// Handles own supplement name availability checks.
/// </summary>
internal sealed class CheckOwnSupplementNameAvailabilityHandler(
    IClientOwnSupplementRepository _clientOwnSupplementRepository)
    : IRequestHandler<CheckOwnSupplementNameAvailabilityQuery, ErrorOr<CheckOwnSupplementNameAvailabilityResponse>>
{
    public async Task<ErrorOr<CheckOwnSupplementNameAvailabilityResponse>> Handle(
        CheckOwnSupplementNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        ClientOwnSupplement? existing = await _clientOwnSupplementRepository.FindByNormalizedNameForUserAsync(
            query.UserId,
            query.NormalizedName,
            query.ExcludeId,
            cancellationToken);

        return new CheckOwnSupplementNameAvailabilityResponse(
            IsAvailable: existing is null,
            ExistingId: existing?.Id);
    }
}
