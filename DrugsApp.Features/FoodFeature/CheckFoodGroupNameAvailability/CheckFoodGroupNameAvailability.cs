using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.CheckFoodGroupNameAvailability;



/// <summary>
/// Handler for checking food group name availability.
/// </summary>
public sealed class CheckFoodGroupNameAvailabilityHandler : IRequestHandler<CheckFoodGroupNameAvailabilityQuery, ErrorOr<CheckFoodGroupNameAvailabilityResponse>>
{
    private readonly IFoodGroupRepository _foodGroupRepository;

    /// <summary>
    /// Handles the processing of checking the availability of a food group name.
    /// </summary>
    /// <param name="foodGroupRepository"></param>
    /// <returns>A response indicating whether the food group name is available, including any conflicting resource ID if applicable.</returns>
    public CheckFoodGroupNameAvailabilityHandler(IFoodGroupRepository foodGroupRepository)
    {
        _foodGroupRepository = foodGroupRepository;
    }

    /// <summary>
    /// Handles the processing of checking the availability of a food group name.
    /// </summary>
    /// <param name="query">The query containing the food group name to be checked and an optional ID to exclude.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A response indicating whether the food group name is available and any conflicting resource ID, if applicable.</returns>
    public async Task<ErrorOr<CheckFoodGroupNameAvailabilityResponse>> Handle(
        CheckFoodGroupNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalizedName = query.Name.Trim();

        FoodGroup? existing = await _foodGroupRepository.GetByNameAsync(
            normalizedName,
            cancellationToken);

        if (existing is null)
        {
            return CheckFoodGroupNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        if (query.ExcludeId is not null && existing.Id == query.ExcludeId.Value)
        {
            return CheckFoodGroupNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        return CheckFoodGroupNameAvailabilityMapping.MapToResponse(
            isAvailable: false,
            conflictingResourceId: existing.Id);
    }
}
