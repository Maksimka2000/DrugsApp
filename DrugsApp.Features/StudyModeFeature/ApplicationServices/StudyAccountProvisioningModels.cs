using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Features.StudyModeFeature.ApplicationServices;

/// <summary>
/// Healthcare provider profile data of a study doctor.
/// </summary>
/// <param name="ProfessionCode">Provider profession.</param>
/// <param name="OtherProfessionLabel">Custom profession label when the profession is other.</param>
/// <param name="WorkplaceLabel">Optional workplace label.</param>
/// <param name="ShortDescription">Optional short description.</param>
public sealed record StudyProviderProfileData(
    HealthcareProviderProfession ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription);

/// <summary>
/// Input of study account provisioning.
/// </summary>
/// <param name="MemberKind">Doctor or patient.</param>
/// <param name="PersonalData">Profile personal data.</param>
/// <param name="ProviderData">Provider profile data; required for doctors, ignored for patients.</param>
/// <param name="PositionX">Initial horizontal canvas coordinate.</param>
/// <param name="PositionY">Initial vertical canvas coordinate.</param>
public sealed record StudyAccountProvisioningDraft(
    StudyMemberKind MemberKind,
    UserProfilePersonalData PersonalData,
    StudyProviderProfileData? ProviderData,
    double PositionX,
    double PositionY);

/// <summary>
/// Output of study account provisioning; carries the plain password exactly once.
/// </summary>
/// <param name="UserId">New study account user identifier.</param>
/// <param name="LoginEmail">Generated login email.</param>
/// <param name="Password">Generated plain password.</param>
public sealed record StudyAccountProvisioningResult(
    Guid UserId,
    string LoginEmail,
    string Password);
