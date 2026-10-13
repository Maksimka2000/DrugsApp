using DrugsApp.Domain.StudyModeFeature.DomainServices;
using DrugsApp.Features.StudyModeFeature.Shared;

namespace DrugsApp.Features.StudyModeFeature.GenerateStudyPatientRandomProfile;

/// <summary>
/// Pure mapping helpers of GenerateStudyPatientRandomProfile (no I/O, no business rules).
/// </summary>
public static class GenerateStudyPatientRandomProfileMapping
{
    /// <summary>
    /// Maps the generated person profile to the HTTP response contract.
    /// </summary>
    public static StudyRandomPatientProfileResponse MapToResponse(this StudyRandomPersonProfile profile) => new(
        FirstName: profile.FirstName,
        LastName: profile.LastName,
        DateOfBirth: profile.DateOfBirth,
        Gender: profile.Gender);
}
