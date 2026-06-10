using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.RegularSupplementsFeature.UpdateRegularSupplement;

/// <summary>
/// Validates update regular supplement request payloads.
/// </summary>
public sealed class UpdateRegularSupplementRequestValidator : AbstractValidator<UpdateRegularSupplementRequest>
{
    public UpdateRegularSupplementRequestValidator()
    {
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
