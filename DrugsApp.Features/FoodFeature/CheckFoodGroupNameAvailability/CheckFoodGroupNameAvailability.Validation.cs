using System;
using FluentValidation;

namespace DrugsApp.Features.FoodFeature.CheckFoodGroupNameAvailability;

/// <summary>
/// Validator for food group name availability request.
/// </summary>
public sealed class CheckFoodGroupNameAvailabilityRequestValidator : AbstractValidator<CheckFoodGroupNameAvailabilityRequest>
{
    public CheckFoodGroupNameAvailabilityRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters.");

        RuleFor(request => request.ExcludeId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("Exclude id must be a valid GUID when provided.");
    }
}
