using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.StudyModeFeature.ApplicationServices;
using DrugsApp.Features.StudyModeFeature.Shared;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyPatient;

/// <summary>
/// Pure mapping helpers of CreateStudyPatient (no I/O, no business rules).
/// </summary>
public static class CreateStudyPatientMapping
{
    /// <summary>
    /// Maps the HTTP request and route values to the MediatR command.
    /// </summary>
    public static CreateStudyPatientCommand MapToCommand(this CreateStudyPatientRequest request, Guid clinicId) => new(
        ClinicId: clinicId,
        Request: request);

    /// <summary>
    /// Maps the HTTP request to the study account provisioning draft.
    /// </summary>
    public static StudyAccountProvisioningDraft MapToDraft(this CreateStudyPatientRequest request) => new(
        MemberKind: StudyMemberKind.Patient,
        PersonalData: new UserProfilePersonalData(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            Enum.Parse<ProfileGender>(request.Gender, ignoreCase: true)),
        ProviderData: null,
        PositionX: request.PositionX,
        PositionY: request.PositionY);

    /// <summary>
    /// Maps the use-case result to the HTTP response contract.
    /// </summary>
    public static CreatedStudyAccountResponse MapToResponse(
        this StudyAccountProvisioningResult result,
        StudyAccountProvisioningDraft draft) => new(
        Doctor: null,
        Patient: new StudyPatientNodeDto(
            UserId: result.UserId,
            FirstName: draft.PersonalData.FirstName.Trim(),
            LastName: draft.PersonalData.LastName.Trim(),
            DateOfBirth: draft.PersonalData.DateOfBirth,
            Gender: draft.PersonalData.Gender.ToString(),
            LoginEmail: result.LoginEmail,
            PositionX: draft.PositionX,
            PositionY: draft.PositionY,
            MedicalSummary: StudyPatientMedicalSummaryReadModel.Empty.MapToDto()),
        Credentials: new StudyAccountCredentialsDto(result.LoginEmail, result.Password));
}
