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
    /// <summary>
    /// Handles the processing of a request to check the availability of a supplement name.
    /// </summary>
    /// <param name="query">The query object containing the supplement name and an optional ID to exclude from the check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a <see cref="CheckSupplementNameAvailabilityResponse"/> which provides the availability status
    /// of the supplement name and any existing supplement ID that matches.</returns>
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
