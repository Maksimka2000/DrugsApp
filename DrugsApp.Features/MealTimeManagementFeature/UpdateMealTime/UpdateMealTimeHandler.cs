using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using DrugsApp.Features.MealTimeManagementFeature.ApplicationServices;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.MealTimeManagementFeature.UpdateMealTime;

/// <summary>
/// Handles partial updates to meal times for the current user.
/// </summary>
internal sealed class UpdateMealTimeHandler(
    IClientMealTimeRepository _mealTimeRepository,
    IClientMealTimeNormalizationService _normalizationService)
    : IRequestHandler<UpdateMealTimeCommand, ErrorOr<MealTimeDto>>
{
    public async Task<ErrorOr<MealTimeDto>> Handle(
        UpdateMealTimeCommand command,
        CancellationToken cancellationToken)
    {
        ClientMealTime? entity = await _mealTimeRepository.GetByIdForUserAsync(
            command.UserId,
            command.MealTimeId,
            tracking: true,
            cancellationToken);

        if (entity is null)
        {
            return Error.NotFound(
                code: "MealTime.NotFound",
                description: "Meal time not found.",
                metadata: new()
                {
                    ["resourceType"] = "ClientMealTime",
                    ["resourceId"] = command.MealTimeId.ToString()
                });
        }

        if (!string.IsNullOrWhiteSpace(command.Request.MealName))
        {
            string normalizedMealName = _normalizationService.NormalizeMealName(command.Request.MealName);
            bool hasDuplicateName = await _mealTimeRepository.ExistsWithNormalizedMealNameAsync(
                command.UserId,
                normalizedMealName,
                command.MealTimeId,
                cancellationToken);
            if (hasDuplicateName)
            {
                return Error.Conflict(
                    code: "MealTime.DuplicateMealName",
                    description: "Meal name already exists for this user.",
                    metadata: new()
                    {
                        ["field"] = "mealName",
                        ["conflictReason"] = "meal_name_duplicate"
                    });
            }

            ErrorOr<Success> updateNameResult = entity.UpdateMealName(command.Request.MealName);
            if (updateNameResult.IsError)
            {
                return updateNameResult.Errors;
            }
        }

        if (command.Request.IntakeTime is not null)
        {
            TimeOnly intakeTime = command.Request.IntakeTime.Value;

            bool hasDuplicateTime = await _mealTimeRepository.ExistsWithIntakeTimeAsync(
                command.UserId,
                intakeTime,
                command.MealTimeId,
                cancellationToken);
            if (hasDuplicateTime)
            {
                return Error.Conflict(
                    code: "MealTime.DuplicateIntakeTime",
                    description: "Intake time already exists for this user.",
                    metadata: new()
                    {
                        ["field"] = "intakeTime",
                        ["conflictReason"] = "intake_time_duplicate"
                    });
            }

            ErrorOr<Success> updateTimeResult = entity.UpdateIntakeTime(intakeTime);
            if (updateTimeResult.IsError)
            {
                return updateTimeResult.Errors;
            }
        }

        return entity.MapToDto();
    }
}
