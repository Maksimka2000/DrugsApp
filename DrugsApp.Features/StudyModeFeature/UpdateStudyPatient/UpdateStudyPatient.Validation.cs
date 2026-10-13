using DrugsApp.Features.StudyModeFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyPatient;

/// <summary>
/// Validator for the study patient update body.
/// </summary>
public sealed class UpdateStudyPatientRequestValidator : AbstractValidator<UpdateStudyPatientRequest>
{
    /// <summary>
    /// Initializes the shared personal-data rules.
    /// </summary>
    public UpdateStudyPatientRequestValidator()
    {
        Include(new StudyPersonalDataRules());
    }
}
