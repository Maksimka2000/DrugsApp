using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.RegularDrugsFeature.UpdateRegularDrug;

/// <summary>
/// Validates update regular drug request payloads.
/// </summary>
public sealed class UpdateRegularDrugRequestValidator : AbstractValidator<UpdateRegularDrugRequest>
{
    public UpdateRegularDrugRequestValidator()
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
