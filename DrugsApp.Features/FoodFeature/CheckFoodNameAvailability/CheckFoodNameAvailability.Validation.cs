using System;
using FluentValidation;

namespace DrugsApp.Features.FoodFeature.CheckFoodNameAvailability;

/// <summary>
/// Validator for food name availability request.
/// </summary>
public sealed class CheckFoodNameAvailabilityRequestValidator : AbstractValidator<CheckFoodNameAvailabilityRequest>
{
    public CheckFoodNameAvailabilityRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(256)
            .WithMessage("Name must not exceed 256 characters.");

        RuleFor(request => request.ExcludeId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("Exclude id must be a valid GUID when provided.");
    }
}
