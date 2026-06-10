using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.RegularFoodFeature.CreateRegularFood;

/// <summary>
/// Validates create regular food request payloads.
/// </summary>
public sealed class CreateRegularFoodRequestValidator : AbstractValidator<CreateRegularFoodRequest>
{
    public CreateRegularFoodRequestValidator()
    {
        RuleFor(request => request.FoodId)
            .NotEmpty();

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
