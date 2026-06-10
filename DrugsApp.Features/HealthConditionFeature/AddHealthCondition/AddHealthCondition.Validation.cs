using FluentValidation;

namespace DrugsApp.Features.HealthConditionFeature.AddHealthCondition;

/// <summary>
/// Validator for add health condition request.
/// </summary>
public sealed class AddHealthConditionRequestValidator : AbstractValidator<AddHealthConditionRequest>
{
    public AddHealthConditionRequestValidator()
    {
        RuleFor(request => request.HealthConditionName)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("healthConditionName must not be empty.")
            .MaximumLength(256);

        When(request => !string.IsNullOrWhiteSpace(request.Description), () =>
        {
            RuleFor(request => request.Description!)
                .MaximumLength(2000);
        });
    }
}
