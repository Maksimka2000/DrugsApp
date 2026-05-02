using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.FoodFeature.UpdateFoodGroup;

/// <summary>
/// Validator for UpdateFoodGroup request payload.
/// </summary>
public sealed class UpdateFoodGroupRequestValidator : AbstractValidator<UpdateFoodGroupRequest>
{
    public UpdateFoodGroupRequestValidator()
    {
        When(
            request => request.Name is not null,
            () =>
            {
                RuleFor(request => request.Name!)
                    .NotEmpty()
                    .WithMessage("Name cannot be empty when provided.")
                    .MaximumLength(200)
                    .WithMessage("Name must not exceed 200 characters.");
            });

        RuleFor(request => request.Members)
            .NotEmpty()
            .WithMessage("Members are required.")
            .Must(members => members.All(id => id != Guid.Empty))
            .WithMessage("All member ids must be valid GUIDs.")
            .Must(members => members.Distinct().Count() == members.Count)
            .WithMessage("Member ids must be unique.");
    }
}
