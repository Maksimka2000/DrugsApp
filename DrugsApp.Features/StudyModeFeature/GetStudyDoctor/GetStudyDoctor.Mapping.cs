using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Features.StudyModeFeature.Shared;

namespace DrugsApp.Features.StudyModeFeature.GetStudyDoctor;

/// <summary>
/// Pure mapping helpers of GetStudyDoctor (no I/O, no business rules).
/// </summary>
public static class GetStudyDoctorMapping
{
    /// <summary>
    /// Maps the member detail read model to the doctor detail DTO.
    /// </summary>
    public static StudyDoctorDetailDto MapToDoctorDetailDto(this StudyMemberDetailReadModel detail) => new(
        UserId: detail.UserId,
        LoginEmail: detail.LoginEmail,
        FirstName: detail.FirstName,
        LastName: detail.LastName,
        DateOfBirth: detail.DateOfBirth,
        Gender: detail.Gender.ToString(),
        ProfessionCode: detail.ProfessionCode?.ToString(),
        OtherProfessionLabel: detail.OtherProfessionLabel,
        WorkplaceLabel: detail.WorkplaceLabel,
        ShortDescription: detail.ShortDescription);
}
