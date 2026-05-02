using FluentValidation;

namespace DrugsApp.Features.FoodFeature.RemoveFoodGroup;

/// <summary>
/// Validator for RemoveFoodGroup request payload.
/// </summary>
public sealed class RemoveFoodGroupRequestValidator : AbstractValidator<RemoveFoodGroupRequest>
{
    public RemoveFoodGroupRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithMessage("Id is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("Id must be a valid GUID.");
    }
}