using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.CheckActiveSubstanceGroupNameAvailability;

/// <summary>
/// Handles validation of active substance group name availability.
/// </summary>
internal sealed class CheckActiveSubstanceGroupNameAvailabilityHandler(IActiveSubstanceGroupRepository _activeSubstanceGroupRepository)
    : IRequestHandler<CheckActiveSubstanceGroupNameAvailabilityQuery, ErrorOr<ActiveSubstanceGroupNameAvailabilityResponse>>
{
    /// <summary>
    /// Validates name availability with optional exclusion.
    /// </summary>
    /// <param name="query">Availability query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<ErrorOr<ActiveSubstanceGroupNameAvailabilityResponse>> Handle(
        CheckActiveSubstanceGroupNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalizedName = query.Name.Trim();

        ActiveSubstanceGroup? existing = await _activeSubstanceGroupRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is null)
        {
            return CheckActiveSubstanceGroupNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingGroupId: null);
        }

        if (query.ExcludeId is not null && query.ExcludeId.Value == existing.Id)
        {
            return CheckActiveSubstanceGroupNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingGroupId: null);
        }

        return CheckActiveSubstanceGroupNameAvailabilityMapping.MapToResponse(
            isAvailable: false,
            conflictingGroupId: existing.Id);
    }
}
