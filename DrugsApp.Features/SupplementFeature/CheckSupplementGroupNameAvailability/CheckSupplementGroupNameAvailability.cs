using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.CheckSupplementGroupNameAvailability;



/// <summary>
/// Handles checking supplement group name availability.
/// </summary>
public sealed class CheckSupplementGroupNameAvailabilityHandler(ISupplementGroupRepository _supplementGroupRepository) : IRequestHandler<CheckSupplementGroupNameAvailabilityQuery, ErrorOr<CheckSupplementGroupNameAvailabilityResponse>>
{

    public async Task<ErrorOr<CheckSupplementGroupNameAvailabilityResponse>> Handle(
        CheckSupplementGroupNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        SupplementGroup? existing = await _supplementGroupRepository.GetByNameAsync(
            query.Name.Trim(),
            cancellationToken);

        if (existing is null || existing.Id == query.ExcludeId)
        {
            return new CheckSupplementGroupNameAvailabilityResponse(
                IsAvailable: true,
                ExistingId: null);
        }

        return new CheckSupplementGroupNameAvailabilityResponse(
            IsAvailable: false,
            ExistingId: existing.Id);
    }
}
