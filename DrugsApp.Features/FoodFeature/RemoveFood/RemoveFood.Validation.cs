using FluentValidation;

namespace DrugsApp.Features.FoodFeature.RemoveFood;

/// <summary>
/// Validator for RemoveFood request payload.
/// </summary>
public sealed class RemoveFoodRequestValidator : AbstractValidator<RemoveFoodRequest>
{
    public RemoveFoodRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithMessage("Id is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("Id must be a valid GUID.");

    }
}