using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using DrugsApp.Features.MealTimeManagementFeature.ApplicationServices;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.MealTimeManagementFeature.CheckMealTimeAvailability;

/// <summary>
/// Handles meal time availability checks for the current user.
/// </summary>
internal sealed class CheckMealTimeAvailabilityHandler(
    IClientMealTimeRepository _mealTimeRepository,
    IClientMealTimeNormalizationService _normalizationService)
    : IRequestHandler<CheckMealTimeAvailabilityQuery, ErrorOr<MealTimeAvailabilityResponse>>
{
    public async Task<ErrorOr<MealTimeAvailabilityResponse>> Handle(
        CheckMealTimeAvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        bool mealNameAvailable = true;
        bool intakeTimeAvailable = true;
        List<string> conflictReasons = [];

        if (!string.IsNullOrWhiteSpace(query.MealName))
        {
            string normalizedMealName = _normalizationService.NormalizeMealName(query.MealName);
            bool hasDuplicateName = await _mealTimeRepository.ExistsWithNormalizedMealNameAsync(
                query.UserId,
                normalizedMealName,
                query.ExcludeId,
                cancellationToken);

            mealNameAvailable = !hasDuplicateName;
            if (hasDuplicateName)
            {
                conflictReasons.Add("meal_name_duplicate");
            }
        }

        if (query.IntakeTime is not null)
        {
            bool hasDuplicateTime = await _mealTimeRepository.ExistsWithIntakeTimeAsync(
                query.UserId,
                query.IntakeTime.Value,
                query.ExcludeId,
                cancellationToken);

            intakeTimeAvailable = !hasDuplicateTime;
            if (hasDuplicateTime)
            {
                conflictReasons.Add("intake_time_duplicate");
            }
        }

        return new MealTimeAvailabilityResponse(
            MealNameAvailable: mealNameAvailable,
            IntakeTimeAvailable: intakeTimeAvailable,
            ConflictReasons: conflictReasons);
    }
}
