using DrugsApp.Features.StudyModeFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyDoctor;

/// <summary>
/// Validator for the study doctor creation body.
/// </summary>
public sealed class CreateStudyDoctorRequestValidator : AbstractValidator<CreateStudyDoctorRequest>
{
    /// <summary>
    /// Initializes personal-data, provider-profile and position rules.
    /// </summary>
    public CreateStudyDoctorRequestValidator()
    {
        Include(new StudyPersonalDataRules());
        Include(new StudyDoctorProfileRules());

        RuleFor(request => request.PositionX)
            .Must(double.IsFinite)
            .WithMessage("positionX must be a finite number.");

        RuleFor(request => request.PositionY)
            .Must(double.IsFinite)
            .WithMessage("positionY must be a finite number.");
    }
}
