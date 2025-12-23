using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.PharmaceuticalFormFeature.CheckPharmaceuticalFormNameAvailability;

/// <summary>
/// Handles checking pharmaceutical form name availability.
/// </summary>
internal sealed class CheckPharmaceuticalFormNameAvailabilityHandler
    : IRequestHandler<CheckPharmaceuticalFormNameAvailabilityQuery, ErrorOr<PharmaceuticalFormNameAvailabilityDto>>
{
    private readonly IPharmaceuticalFormRepository _pharmaceuticalFormRepository;

    public CheckPharmaceuticalFormNameAvailabilityHandler(IPharmaceuticalFormRepository pharmaceuticalFormRepository)
    {
        _pharmaceuticalFormRepository = pharmaceuticalFormRepository;
    }

    /// <summary>
    /// Checks if a pharmaceutical form name is available, applying optional exclusion semantics.
    /// </summary>
    /// <param name="query">Availability query containing name and optional exclude id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Availability result describing whether the name is available.</returns>
    public async Task<ErrorOr<PharmaceuticalFormNameAvailabilityDto>> Handle(
        CheckPharmaceuticalFormNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalizedName = query.Name.Trim();

        PharmaceuticalForm? existing = await _pharmaceuticalFormRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is null)
        {
            return CheckPharmaceuticalFormNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        if (query.ExcludeId is not null && query.ExcludeId.Value == existing.Id)
        {
            return CheckPharmaceuticalFormNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        return CheckPharmaceuticalFormNameAvailabilityMapping.MapToResponse(
            isAvailable: false,
            conflictingResourceId: existing.Id);
    }
}


