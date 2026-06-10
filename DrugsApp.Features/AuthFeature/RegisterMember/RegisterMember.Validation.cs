using FluentValidation;
using System;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Features.AuthFeature.RegisterMember;

public sealed class RegisterMemberRequestValidator : AbstractValidator<RegisterMemberRequest>
{
    public RegisterMemberRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(request => request.ConfirmPassword)
            .Equal(r => r.Password)
            .WithMessage("Passwords do not match.");

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
