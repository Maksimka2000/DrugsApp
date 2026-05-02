using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.FoodFeature.UpdateFood;

/// <summary>
/// Validator for UpdateFood request payload.
/// </summary>
public sealed class UpdateFoodRequestValidator : AbstractValidator<UpdateFoodRequest>
{
    public UpdateFoodRequestValidator()
    {
        When(
            request => request.Name is not null,
            () =>
            {
                RuleFor(request => request.Name!)
                    .NotEmpty()
                    .WithMessage("Name cannot be empty when provided.")
                    .MaximumLength(256)
                    .WithMessage("Name must not exceed 256 characters.");
            });

        When(
            request => request.Aliases is not null,
            () =>
            {
                RuleFor(request => request.Aliases!)
                    .Must(aliases =>
                        aliases.Distinct(StringComparer.OrdinalIgnoreCase).Count() == aliases.Count)
                    .WithMessage("Aliases must unique.");

                RuleForEach(request => request.Aliases!)
                    .NotNull() 
                    .WithMessage("Alias cannot be null.")
                    .NotEmpty()
                    .WithMessage("Alias cannot be empty.")
                    .MaximumLength(256)
                    .WithMessage("Each alias must not exceed 256 characters.");

                RuleFor(request => request)
                    .Must(request => request.Aliases!.All(alias =>
                        request.Name is null ||
                        !string.Equals(alias.Trim(), request.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                    .WithMessage("Aliases must differ from the canonical name.");
            });
    }
}
