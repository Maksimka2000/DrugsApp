using System;
using DrugsApp.Domain.UserProfileFeature.Enums;
using FluentValidation;

namespace DrugsApp.Features.UserProfileFeature.UpdateUserProfile;

public sealed class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(r => r.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(r => r.DateOfBirth)
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must not be in the future.");

        RuleFor(r => r.Gender)
            .NotEmpty()
            .Must(value => Enum.TryParse<ProfileGender>(value, true, out _))
            .WithMessage("Gender must be one of: female, male, prefer_not_to_say.");
    }
}
