using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.FoodFeature.AddFoodGroup;

/// <summary>
/// Validator for AddFoodGroup request payload.
/// </summary>
public sealed class AddFoodGroupRequestValidator : AbstractValidator<AddFoodGroupRequest>
{
    public AddFoodGroupRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters.");

        RuleFor(request => request.Members)
            .NotEmpty()
            .WithMessage("At least one food id is required.")
            .Must(members => members.All(id => id != Guid.Empty))
            .WithMessage("All member ids must be valid GUIDs.")
            .Must(members => members.Distinct().Count() == members.Count)
            .WithMessage("Member ids must be unique.");
    }
}
