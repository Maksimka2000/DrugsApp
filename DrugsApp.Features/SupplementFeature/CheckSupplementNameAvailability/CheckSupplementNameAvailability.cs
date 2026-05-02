using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.SupplementFeature.CheckSupplementNameAvailability;



/// <summary>
/// Handles checking supplement name availability.
/// </summary>
public sealed class CheckSupplementNameAvailabilityHandler(ISupplementRepository _supplementRepository) : IRequestHandler<CheckSupplementNameAvailabilityQuery, ErrorOr<CheckSupplementNameAvailabilityResponse>>
{
    public async Task<ErrorOr<CheckSupplementNameAvailabilityResponse>> Handle(
        CheckSupplementNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        Supplement? existing = await _supplementRepository.GetByNameAsync(
            query.Name.Trim(),
            cancellationToken);

        if (existing is null || existing.Id == query.ExcludeId)
        {
            return new CheckSupplementNameAvailabilityResponse(
                IsAvailable: true,
                ExistingId: null);
        }

        return new CheckSupplementNameAvailabilityResponse(
            IsAvailable: false,
            ExistingId: existing.Id);
    }
}
