namespace DrugsApp.Features.StudyModeFeature.Shared;

/// <summary>
/// Newly provisioned study account node together with its one-time plain credentials.
/// </summary>
/// <param name="Doctor">Doctor node; set for doctors.</param>
/// <param name="Patient">Patient node; set for patients.</param>
/// <param name="Credentials">Generated login and password.</param>
public sealed record CreatedStudyAccountResponse(
    StudyDoctorNodeDto? Doctor,
    StudyPatientNodeDto? Patient,
    StudyAccountCredentialsDto Credentials);

/// <summary>
/// Plain credentials of a study account.
/// </summary>
/// <param name="LoginEmail">Login email.</param>
/// <param name="Password">Plain generated password.</param>
public sealed record StudyAccountCredentialsDto(
    string LoginEmail,
    string Password);

/// <summary>
/// Study doctor data shown in the edit modal.
/// </summary>
/// <param name="UserId">Study account user identifier.</param>
/// <param name="LoginEmail">Generated login email.</param>
/// <param name="FirstName">Profile first name.</param>
/// <param name="LastName">Profile last name.</param>
/// <param name="DateOfBirth">Profile date of birth.</param>
/// <param name="Gender">Profile gender.</param>
/// <param name="ProfessionCode">Provider profession code.</param>
/// <param name="OtherProfessionLabel">Custom profession label.</param>
/// <param name="WorkplaceLabel">Workplace label.</param>
/// <param name="ShortDescription">Provider description.</param>
public sealed record StudyDoctorDetailDto(
    Guid UserId,
    string LoginEmail,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription);

/// <summary>
/// Study patient data shown on the profile tab.
/// </summary>
/// <param name="UserId">Study account user identifier.</param>
/// <param name="LoginEmail">Generated login email.</param>
/// <param name="FirstName">Profile first name.</param>
/// <param name="LastName">Profile last name.</param>
/// <param name="DateOfBirth">Profile date of birth.</param>
/// <param name="Gender">Profile gender.</param>
/// <param name="MedicalSummary">Medical tab badges.</param>
public sealed record StudyPatientDetailDto(
    Guid UserId,
    string LoginEmail,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    StudyPatientMedicalSummaryDto MedicalSummary);
