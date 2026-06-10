using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Domain.MealTimeManagementFeature.Repositories;
using DrugsApp.Features.MealTimeManagementFeature.ApplicationServices;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.MealTimeManagementFeature.CreateMealTime;

/// <summary>
/// Handles creation of meal times for the current user.
/// </summary>
internal sealed class CreateMealTimeHandler(
    IClientMealTimeRepository _mealTimeRepository,
    IClientMealTimeNormalizationService _normalizationService)
    : IRequestHandler<CreateMealTimeCommand, ErrorOr<MealTimeDto>>
{
    public async Task<ErrorOr<MealTimeDto>> Handle(
        CreateMealTimeCommand command,
        CancellationToken cancellationToken)
    {
        MealTimeMappings.TryParseIntakeTime(command.Request.IntakeTime, out TimeOnly intakeTime);

        string normalizedMealName = _normalizationService.NormalizeMealName(command.Request.MealName);

        bool hasDuplicateName = await _mealTimeRepository.ExistsWithNormalizedMealNameAsync(
            command.UserId,
            normalizedMealName,
            excludeId: null,
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

        bool hasDuplicateTime = await _mealTimeRepository.ExistsWithIntakeTimeAsync(
            command.UserId,
            intakeTime,
            excludeId: null,
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

        ErrorOr<ClientMealTime> created = ClientMealTime.Create(
            command.UserId,
            command.Request.MealName,
            intakeTime);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _mealTimeRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDto();
    }
}
