using System;
using FluentValidation;

namespace DrugsApp.Features.HealthConditionFeature.AddUserHealthCondition;

/// <summary>
/// Validator for adding user health conditions.
/// </summary>
public sealed class AddUserHealthConditionRequestValidator : AbstractValidator<AddUserHealthConditionRequest>
{
    public AddUserHealthConditionRequestValidator()
    {
        RuleFor(request => request.HealthConditionId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("healthConditionId must not be an empty Guid.");

        RuleFor(request => request.CustomHealthConditionName)
            .Cascade(CascadeMode.Stop)
            .Must(name => name is null || !string.IsNullOrWhiteSpace(name))
            .WithMessage("customHealthConditionName must not be whitespace.")
            .MaximumLength(256);

        When(request => !string.IsNullOrWhiteSpace(request.Note), () =>
        {
            RuleFor(request => request.Note!)
                .MaximumLength(2000);
        });

        RuleFor(request => request)
            .Must(request =>
                request.HealthConditionId is not null && request.CustomHealthConditionName is null ||
                request.HealthConditionId is null && !string.IsNullOrWhiteSpace(request.CustomHealthConditionName))
            .WithMessage("Exactly one of healthConditionId or customHealthConditionName must be provided.");
    }
}
