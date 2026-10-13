using DrugsApp.Features.StudyModeFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyClinic;

/// <summary>
/// Validator for the study clinic creation body.
/// </summary>
public sealed class CreateStudyClinicRequestValidator : AbstractValidator<CreateStudyClinicRequest>
{
    /// <summary>
    /// Initializes name and scenario description rules.
    /// </summary>
    public CreateStudyClinicRequestValidator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name is required.")
            .Must(name => name is null || name.Trim().Length is >= 3 and <= 120)
            .WithMessage("Name must contain between 3 and 120 characters.");

        When(
            request => !string.IsNullOrWhiteSpace(request.Visibility),
            () =>
            {
                RuleFor(request => request.Visibility)
                    .Must(visibility => StudyModeMappings.MapToVisibility(visibility) is not null)
                    .WithMessage("Visibility must be public or private.");
            });

        When(
            request => !string.IsNullOrWhiteSpace(request.ScenarioDescription),
            () =>
            {
                RuleFor(request => request.ScenarioDescription)
                    .MaximumLength(2000)
                    .WithMessage("Scenario description must not exceed 2000 characters.");
            });
    }
}
