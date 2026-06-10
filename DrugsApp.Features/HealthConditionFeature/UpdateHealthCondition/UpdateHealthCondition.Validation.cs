using FluentValidation;

namespace DrugsApp.Features.HealthConditionFeature.UpdateHealthCondition;

/// <summary>
/// Validator for updating a health condition.
/// </summary>
public sealed class UpdateHealthConditionRequestValidator : AbstractValidator<UpdateHealthConditionRequest>
{
    public UpdateHealthConditionRequestValidator()
    {
        RuleFor(request => request.HealthConditionName)
            .Cascade(CascadeMode.Stop)
            .Must(name => string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(name?.Trim()))
            .WithMessage("healthConditionName must not be whitespace.")
            .MaximumLength(256);

        When(request => request.Description is not null, () =>
        {
            RuleFor(request => request.Description!)
                .MaximumLength(2000);
        });

        RuleFor(request => request)
            .Must(request => !string.IsNullOrWhiteSpace(request.HealthConditionName) || request.Description is not null)
            .WithMessage("At least one field must be provided.");
    }
}
