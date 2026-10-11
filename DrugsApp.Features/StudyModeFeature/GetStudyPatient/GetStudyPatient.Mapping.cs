using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Features.StudyModeFeature.Shared;

namespace DrugsApp.Features.StudyModeFeature.GetStudyPatient;

/// <summary>
/// Pure mapping helpers of GetStudyPatient (no I/O, no business rules).
/// </summary>
public static class GetStudyPatientMapping
{
    /// <summary>
    /// Maps the member detail read model to the patient detail DTO.
    /// </summary>
    public static StudyPatientDetailDto MapToPatientDetailDto(this StudyMemberDetailReadModel detail) => new(
        UserId: detail.UserId,
        LoginEmail: detail.LoginEmail,
        FirstName: detail.FirstName,
        LastName: detail.LastName,
        DateOfBirth: detail.DateOfBirth,
        Gender: detail.Gender.ToString(),
        MedicalSummary: detail.MapToMedicalSummaryDto());
}
