using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.StudyModeFeature.Shared;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyPatient;

/// <summary>
/// Pure mapping helpers of UpdateStudyPatient (no I/O, no business rules).
/// </summary>
public static class UpdateStudyPatientMapping
{
    /// <summary>
    /// Maps the HTTP request and route values to the MediatR command.
    /// </summary>
    public static UpdateStudyPatientCommand MapToCommand(
        this UpdateStudyPatientRequest request,
        Guid clinicId,
        Guid patientUserId) => new(
        ClinicId: clinicId,
        PatientUserId: patientUserId,
        Request: request);

    /// <summary>
    /// Maps the HTTP request to user profile personal data.
    /// </summary>
    public static UserProfilePersonalData MapToPersonalData(this UpdateStudyPatientRequest request) => new(
        request.FirstName,
        request.LastName,
        request.DateOfBirth,
        Enum.Parse<ProfileGender>(request.Gender, ignoreCase: true));

    /// <summary>
    /// Maps the use-case result to the HTTP response contract.
    /// </summary>
    public static StudyPatientNodeDto MapToResponse(
        this UserProfilePersonalData personalData,
        StudyMemberDetailReadModel detail) => new(
        UserId: detail.UserId,
        FirstName: personalData.FirstName.Trim(),
        LastName: personalData.LastName.Trim(),
        DateOfBirth: personalData.DateOfBirth,
        Gender: personalData.Gender.ToString(),
        LoginEmail: detail.LoginEmail,
        PositionX: detail.PositionX,
        PositionY: detail.PositionY,
        MedicalSummary: detail.MapToMedicalSummaryDto());
}
