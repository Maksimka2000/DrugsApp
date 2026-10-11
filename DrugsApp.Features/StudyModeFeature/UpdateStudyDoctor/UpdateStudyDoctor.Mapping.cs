using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.StudyModeFeature.Shared;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyDoctor;

/// <summary>
/// Pure mapping helpers of UpdateStudyDoctor (no I/O, no business rules).
/// </summary>
public static class UpdateStudyDoctorMapping
{
    /// <summary>
    /// Maps the HTTP request and route values to the MediatR command.
    /// </summary>
    public static UpdateStudyDoctorCommand MapToCommand(
        this UpdateStudyDoctorRequest request,
        Guid clinicId,
        Guid doctorUserId) => new(
        ClinicId: clinicId,
        DoctorUserId: doctorUserId,
        Request: request);

    /// <summary>
    /// Maps the HTTP request to user profile personal data.
    /// </summary>
    public static UserProfilePersonalData MapToPersonalData(this UpdateStudyDoctorRequest request) => new(
        request.FirstName,
        request.LastName,
        request.DateOfBirth,
        Enum.Parse<ProfileGender>(request.Gender, ignoreCase: true));

    /// <summary>
    /// Maps the HTTP request to the provider profile upsert payload.
    /// </summary>
    public static HealthcareProviderProfileUpsertRequest MapToProviderRequest(this UpdateStudyDoctorRequest request) => new(
        Enum.Parse<HealthcareProviderProfession>(request.ProfessionCode, ignoreCase: true),
        request.OtherProfessionLabel,
        request.WorkplaceLabel,
        request.ShortDescription,
        OnboardingCompletedAtUtc: null);

    /// <summary>
    /// Maps the use-case result to the HTTP response contract.
    /// </summary>
    public static StudyDoctorNodeDto MapToResponse(this UpdateStudyDoctorRequest request, StudyClinicMember member) => new(
        UserId: member.UserId,
        FirstName: request.FirstName.Trim(),
        LastName: request.LastName.Trim(),
        ProfessionCode: Enum.Parse<HealthcareProviderProfession>(request.ProfessionCode, ignoreCase: true).ToString(),
        OtherProfessionLabel: request.OtherProfessionLabel,
        WorkplaceLabel: request.WorkplaceLabel,
        LoginEmail: member.LoginEmail,
        PositionX: member.PositionX,
        PositionY: member.PositionY);
}
