using DrugsApp.Domain.UserProfileFeature.Enums;
using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.Shared;

/// <summary>
/// Personal-data shape shared by every study doctor and study patient create/update request.
/// </summary>
public interface IStudyPersonalDataRequest
{
    /// <summary>
    /// Profile first name.
    /// </summary>
    string FirstName { get; }

    /// <summary>
    /// Profile last name.
    /// </summary>
    string LastName { get; }

    /// <summary>
    /// Profile date of birth.
    /// </summary>
    DateOnly DateOfBirth { get; }

    /// <summary>
    /// Profile gender as a <see cref="ProfileGender"/> name.
    /// </summary>
    string Gender { get; }
}

/// <summary>
/// Reusable FluentValidation rule set for study account personal data, aligned with the user profile limits;
/// included by the study doctor and study patient validators.
/// </summary>
public sealed class StudyPersonalDataRules : AbstractValidator<IStudyPersonalDataRequest>
{
    /// <summary>
    /// Initializes the personal-data rules.
    /// </summary>
    public StudyPersonalDataRules()
    {
        RuleFor(request => request.FirstName)
            .Must(firstName => !string.IsNullOrWhiteSpace(firstName))
            .WithMessage("First name is required.")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(request => request.LastName)
            .Must(lastName => !string.IsNullOrWhiteSpace(lastName))
            .WithMessage("Last name is required.")
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters.");

        RuleFor(request => request.DateOfBirth)
            .Must(dateOfBirth => dateOfBirth <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must not be in the future.");

        RuleFor(request => request.Gender)
            .Must(gender => !string.IsNullOrWhiteSpace(gender)
                && Enum.TryParse<ProfileGender>(gender, ignoreCase: true, out _))
            .WithMessage("Gender must be one of: female, male, prefer_not_to_say.");
    }
}
