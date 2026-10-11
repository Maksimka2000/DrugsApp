using DrugsApp.Features.StudyModeFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyPatient;

/// <summary>
/// Validator for the study patient creation body.
/// </summary>
public sealed class CreateStudyPatientRequestValidator : AbstractValidator<CreateStudyPatientRequest>
{
    /// <summary>
    /// Initializes personal-data and position rules.
    /// </summary>
    public CreateStudyPatientRequestValidator()
    {
        Include(new StudyPersonalDataRules());

        RuleFor(request => request.PositionX)
            .Must(double.IsFinite)
            .WithMessage("positionX must be a finite number.");

        RuleFor(request => request.PositionY)
            .Must(double.IsFinite)
            .WithMessage("positionY must be a finite number.");
    }
}
