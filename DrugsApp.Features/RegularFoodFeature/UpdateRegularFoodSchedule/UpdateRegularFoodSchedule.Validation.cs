using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.RegularFoodFeature.UpdateRegularFoodSchedule;

/// <summary>
/// Validates update regular food schedule request payloads.
/// </summary>
public sealed class UpdateRegularFoodScheduleRequestValidator : AbstractValidator<UpdateRegularFoodScheduleRequest>
{
    public UpdateRegularFoodScheduleRequestValidator()
    {
        When(
            request => request.StartDate.HasValue && request.EndDate.HasValue,
            () =>
            {
                RuleFor(request => request.EndDate!.Value)
                    .GreaterThanOrEqualTo(request => request.StartDate!.Value);
            });

        When(
            request => request.IntakeTimes is not null,
            () =>
            {
                RuleFor(request => request.IntakeTimes!)
                    .Must(intakeTimes => intakeTimes.Distinct().Count() == intakeTimes.Count)
                    .WithMessage("Intake times must be unique.");
            });
    }
}
