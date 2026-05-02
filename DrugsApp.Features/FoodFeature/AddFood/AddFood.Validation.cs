using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.FoodFeature.AddFood;

/// <summary>
/// Validator for AddFood request payload.
/// </summary>
public sealed class AddFoodRequestValidator : AbstractValidator<AddFoodRequest>
{
    public AddFoodRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(256)
            .WithMessage("Name must not exceed 256 characters.");

        When(
            request => request.Aliases is not null,
            () =>
            {
                RuleFor(request => request.Aliases!)
                    .Must(aliases =>
                        aliases.All(alias => !string.IsNullOrWhiteSpace(alias)) &&
                        aliases.Distinct(StringComparer.OrdinalIgnoreCase).Count() == aliases.Count)
                    .WithMessage("Aliases must be non-empty, unique, and trimmed.");

                RuleForEach(request => request.Aliases!)
                    .MaximumLength(256)
                    .WithMessage("Each alias must not exceed 256 characters.");

                RuleFor(request => request)
                    .Must(request => request.Aliases!.All(alias =>
                        !string.Equals(alias.Trim(), request.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                    .WithMessage("Aliases must differ from the canonical name.");
            });
    }
}
