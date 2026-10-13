using DrugsApp.Domain.UserProfileFeature.Enums;
using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.Shared;

/// <summary>
/// Provider-profile shape shared by the study doctor create and update requests.
/// </summary>
public interface IStudyDoctorProfileRequest
{
    /// <summary>
    /// Provider profession as a <see cref="HealthcareProviderProfession"/> name.
    /// </summary>
    string ProfessionCode { get; }

    /// <summary>
    /// Custom profession label; required when the profession is other.
    /// </summary>
    string? OtherProfessionLabel { get; }

    /// <summary>
    /// Optional workplace label.
    /// </summary>
    string? WorkplaceLabel { get; }

    /// <summary>
    /// Optional short description.
    /// </summary>
    string? ShortDescription { get; }
}

/// <summary>
/// Reusable FluentValidation rule set for study doctor provider data, reusing the limits of the client
/// healthcare-provider profile validator.
/// </summary>
public sealed class StudyDoctorProfileRules : AbstractValidator<IStudyDoctorProfileRequest>
{
    /// <summary>
    /// Initializes the provider-profile rules.
    /// </summary>
    public StudyDoctorProfileRules()
    {
        RuleFor(request => request.ProfessionCode)
            .Must(professionCode => !string.IsNullOrWhiteSpace(professionCode)
                && Enum.TryParse<HealthcareProviderProfession>(professionCode, ignoreCase: true, out _))
            .WithMessage("Profession code must be one of: doctor, nurse, pharmacist, dietitian, therapist, other.");

        When(
            request => string.Equals(
                request.ProfessionCode,
                nameof(HealthcareProviderProfession.other),
                StringComparison.OrdinalIgnoreCase),
            () =>
            {
                RuleFor(request => request.OtherProfessionLabel)
                    .Must(label => !string.IsNullOrWhiteSpace(label))
                    .WithMessage("Other profession label is required when the profession is other.");
            });

        When(
            request => !string.IsNullOrWhiteSpace(request.OtherProfessionLabel),
            () =>
            {
                RuleFor(request => request.OtherProfessionLabel)
                    .MaximumLength(100)
                    .WithMessage("Other profession label must not exceed 100 characters.");
            });

        When(
            request => !string.IsNullOrWhiteSpace(request.WorkplaceLabel),
            () =>
            {
                RuleFor(request => request.WorkplaceLabel)
                    .MaximumLength(150)
                    .WithMessage("Workplace label must not exceed 150 characters.");
            });

        When(
            request => !string.IsNullOrWhiteSpace(request.ShortDescription),
            () =>
            {
                RuleFor(request => request.ShortDescription)
                    .MaximumLength(500)
                    .WithMessage("Short description must not exceed 500 characters.");
            });
    }
}
