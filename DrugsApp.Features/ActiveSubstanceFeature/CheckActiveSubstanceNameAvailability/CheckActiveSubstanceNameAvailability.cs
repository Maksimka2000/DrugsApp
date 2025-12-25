using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.ActiveSubstanceFeature.CheckActiveSubstanceNameAvailability;

/// <summary>
/// Handles checking active substance name availability.
/// </summary>
internal sealed class CheckActiveSubstanceNameAvailabilityHandler
    : IRequestHandler<CheckActiveSubstanceNameAvailabilityQuery, ErrorOr<ActiveSubstanceNameAvailabilityResponse>>
{
    private readonly IActiveSubstanceRepository _activeSubstanceRepository;

    public CheckActiveSubstanceNameAvailabilityHandler(IActiveSubstanceRepository activeSubstanceRepository)
    {
        _activeSubstanceRepository = activeSubstanceRepository;
    }

    /// <summary>
    /// Checks if an active substance name is available, applying optional exclusion semantics.
    /// </summary>
    /// <param name="query">Availability query containing name and optional exclude id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Availability result describing whether the name is available.</returns>
    public async Task<ErrorOr<ActiveSubstanceNameAvailabilityResponse>> Handle(
        CheckActiveSubstanceNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalizedName = query.Name.Trim();

        ActiveSubstance? existing = await _activeSubstanceRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is null)
        {
            return CheckActiveSubstanceNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        if (query.ExcludeId is not null && query.ExcludeId.Value == existing.Id)
        {
            return CheckActiveSubstanceNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        return CheckActiveSubstanceNameAvailabilityMapping.MapToResponse(
            isAvailable: false,
            conflictingResourceId: existing.Id);
    }
}


