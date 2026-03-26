using System;
using FluentValidation;

namespace DrugsApp.Features.ActiveSubstanceFeature.CheckActiveSubstanceNameAvailability;

/// <summary>
/// Validator for CheckActiveSubstanceNameAvailability query parameters.
/// </summary>
public sealed class CheckActiveSubstanceNameAvailabilityRequestValidator
    : AbstractValidator<CheckActiveSubstanceNameAvailabilityRequest>
{
    public CheckActiveSubstanceNameAvailabilityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be whitespace.")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters.");

        When(x => x.ExcludeId is not null, () => RuleFor(x => x.ExcludeId)
            .NotEqual(Guid.Empty)
            .WithMessage("ExcludeId must not be an empty Guid."));
    }
}


