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

    public CheckFoodGroupNameAvailabilityHandler(IFoodGroupRepository foodGroupRepository)
    {
        _foodGroupRepository = foodGroupRepository;
    }

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
