using DrugsApp.Features.StudyModeFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyClinicVisibility;

/// <summary>
/// Validator for the clinic visibility change body.
/// </summary>
public sealed class UpdateStudyClinicVisibilityRequestValidator : AbstractValidator<UpdateStudyClinicVisibilityRequest>
{
    /// <summary>
    /// Initializes the visibility rule.
    /// </summary>
    public UpdateStudyClinicVisibilityRequestValidator()
    {
        RuleFor(request => request.Visibility)
            .Must(visibility => StudyModeMappings.MapToVisibility(visibility) is not null)
            .WithMessage("Visibility must be public or private.");
    }
}
