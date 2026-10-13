using DrugsApp.Features.StudyModeFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyDoctor;

/// <summary>
/// Validator for the study doctor update body.
/// </summary>
public sealed class UpdateStudyDoctorRequestValidator : AbstractValidator<UpdateStudyDoctorRequest>
{
    /// <summary>
    /// Initializes the shared personal-data and provider-profile rules.
    /// </summary>
    public UpdateStudyDoctorRequestValidator()
    {
        Include(new StudyPersonalDataRules());
        Include(new StudyDoctorProfileRules());
    }
}
