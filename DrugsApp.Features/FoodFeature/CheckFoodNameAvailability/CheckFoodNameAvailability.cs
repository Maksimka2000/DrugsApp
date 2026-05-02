using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.FoodFeature.CheckFoodNameAvailability;



/// <summary>
/// Handler for checking food name availability.
/// </summary>
public sealed class CheckFoodNameAvailabilityHandler : IRequestHandler<CheckFoodNameAvailabilityQuery, ErrorOr<CheckFoodNameAvailabilityResponse>>
{
    private readonly IFoodRepository _foodRepository;

    public CheckFoodNameAvailabilityHandler(IFoodRepository foodRepository)
    {
        _foodRepository = foodRepository;
    }

    public async Task<ErrorOr<CheckFoodNameAvailabilityResponse>> Handle(
        CheckFoodNameAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        string normalizedName = query.Name.Trim();

        Food? existing = await _foodRepository.GetByNameAsync(normalizedName, cancellationToken);

        if (existing is null)
        {
            return CheckFoodNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        if (query.ExcludeId is not null && existing.Id == query.ExcludeId.Value)
        {
            return CheckFoodNameAvailabilityMapping.MapToResponse(
                isAvailable: true,
                conflictingResourceId: null);
        }

        return CheckFoodNameAvailabilityMapping.MapToResponse(
            isAvailable: false,
            conflictingResourceId: existing.Id);
    }
}
