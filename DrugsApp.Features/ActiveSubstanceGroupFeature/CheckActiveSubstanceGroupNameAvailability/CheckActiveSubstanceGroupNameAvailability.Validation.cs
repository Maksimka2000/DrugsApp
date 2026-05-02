using System;
using FluentValidation;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.CheckActiveSubstanceGroupNameAvailability;

/// <summary>
/// Validator for group name availability request.
/// </summary>
public sealed class CheckActiveSubstanceGroupNameAvailabilityRequestValidator : AbstractValidator<CheckActiveSubstanceGroupNameAvailabilityRequest>
{
    public CheckActiveSubstanceGroupNameAvailabilityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters.");

        When(x => x.ExcludeId is not null, () =>
        {
            RuleFor(x => x.ExcludeId!.Value)
                .NotEqual(Guid.Empty)
                .WithMessage("exclude_id must be a non-empty GUID when provided.");
        });
    }
}
