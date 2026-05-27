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
    /// <summary>
    /// Handles checking the availability of a supplement group name.
    /// </summary>
    /// <param name="query">The request containing the name to check and an optional ID to exclude from the search.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A response indicating whether the name is available and, if unavailable, the ID of the existing supplement group.</returns>
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
