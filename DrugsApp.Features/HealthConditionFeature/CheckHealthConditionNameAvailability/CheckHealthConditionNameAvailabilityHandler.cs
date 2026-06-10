using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthConditionFeature.CheckHealthConditionNameAvailability;

/// <summary>
/// Handles catalog health condition name availability checks.
/// </summary>
public sealed class CheckHealthConditionNameAvailabilityHandler(
    IHealthConditionRepository _healthConditionRepository)
    : IRequestHandler<CheckHealthConditionNameAvailabilityQuery, ErrorOr<CheckHealthConditionNameAvailabilityResponse>>
{
    public async Task<ErrorOr<CheckHealthConditionNameAvailabilityResponse>> Handle(
        CheckHealthConditionNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        HealthCondition? existing = await _healthConditionRepository.GetByNormalizedNameAsync(
            query.NormalizedHealthConditionName,
            cancellationToken);

        if (existing is null || existing.Id == query.ExcludeId)
        {
            return new CheckHealthConditionNameAvailabilityResponse(
                IsAvailable: true,
                ConflictingResourceId: null);
        }

        return new CheckHealthConditionNameAvailabilityResponse(
            IsAvailable: false,
            ConflictingResourceId: existing.Id);
    }
}
