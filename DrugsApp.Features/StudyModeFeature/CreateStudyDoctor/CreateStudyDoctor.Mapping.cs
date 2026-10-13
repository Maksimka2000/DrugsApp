using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.StudyModeFeature.ApplicationServices;
using DrugsApp.Features.StudyModeFeature.Shared;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyDoctor;

/// <summary>
/// Pure mapping helpers of CreateStudyDoctor (no I/O, no business rules).
/// </summary>
public static class CreateStudyDoctorMapping
{
    /// <summary>
    /// Maps the HTTP request and route values to the MediatR command.
    /// </summary>
    public static CreateStudyDoctorCommand MapToCommand(this CreateStudyDoctorRequest request, Guid clinicId) => new(
        ClinicId: clinicId,
        Request: request);

    /// <summary>
    /// Maps the HTTP request to the study account provisioning draft.
    /// </summary>
    public static StudyAccountProvisioningDraft MapToDraft(this CreateStudyDoctorRequest request) => new(
        MemberKind: StudyMemberKind.Doctor,
        PersonalData: new UserProfilePersonalData(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            Enum.Parse<ProfileGender>(request.Gender, ignoreCase: true)),
        ProviderData: new StudyProviderProfileData(
            Enum.Parse<HealthcareProviderProfession>(request.ProfessionCode, ignoreCase: true),
            request.OtherProfessionLabel,
            request.WorkplaceLabel,
            request.ShortDescription),
        PositionX: request.PositionX,
        PositionY: request.PositionY);

    /// <summary>
    /// Maps the use-case result to the HTTP response contract.
    /// </summary>
    public static CreatedStudyAccountResponse MapToResponse(
        this StudyAccountProvisioningResult result,
        StudyAccountProvisioningDraft draft) => new(
        Doctor: new StudyDoctorNodeDto(
            UserId: result.UserId,
            FirstName: draft.PersonalData.FirstName.Trim(),
            LastName: draft.PersonalData.LastName.Trim(),
            ProfessionCode: draft.ProviderData?.ProfessionCode.ToString(),
            OtherProfessionLabel: draft.ProviderData?.OtherProfessionLabel,
            WorkplaceLabel: draft.ProviderData?.WorkplaceLabel,
            LoginEmail: result.LoginEmail,
            PositionX: draft.PositionX,
            PositionY: draft.PositionY),
        Patient: null,
        Credentials: new StudyAccountCredentialsDto(result.LoginEmail, result.Password));
}
