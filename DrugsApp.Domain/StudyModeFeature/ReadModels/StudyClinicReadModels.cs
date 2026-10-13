using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Domain.StudyModeFeature.ReadModels;

/// <summary>
/// Search row of a study clinic with its member counts per kind.
/// </summary>
/// <param name="Id">Clinic identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="ScenarioDescription">Optional scenario text.</param>
/// <param name="DoctorCount">Number of doctor members.</param>
/// <param name="PatientCount">Number of patient members.</param>
/// <param name="CreatedAtUtc">Creation timestamp.</param>
/// <param name="Visibility">Public or private.</param>
/// <param name="CreatedByUserId">Creator of the clinic.</param>
public sealed record StudyClinicSummary(
    Guid Id,
    string Name,
    string? ScenarioDescription,
    int DoctorCount,
    int PatientCount,
    DateTime CreatedAtUtc,
    StudyClinicVisibility Visibility,
    Guid CreatedByUserId);

/// <summary>
/// Header data of a study clinic.
/// </summary>
/// <param name="Id">Clinic identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="ScenarioDescription">Optional scenario text.</param>
/// <param name="CreatedAtUtc">Creation timestamp.</param>
/// <param name="Visibility">Public or private.</param>
/// <param name="CreatedByUserId">Creator of the clinic.</param>
public sealed record StudyClinicHeaderReadModel(
    Guid Id,
    string Name,
    string? ScenarioDescription,
    DateTime CreatedAtUtc,
    StudyClinicVisibility Visibility,
    Guid CreatedByUserId);

/// <summary>
/// Medical record counters of a study patient shown as card and tab badges.
/// </summary>
/// <param name="AllergyCount">Sum of drug, supplement, food and other allergies.</param>
/// <param name="HealthConditionCount">Number of user health conditions.</param>
/// <param name="DrugProblemCount">Number of drug problems.</param>
/// <param name="RegularDrugCount">Number of live regular drugs.</param>
/// <param name="RegularSupplementCount">Number of live regular supplements.</param>
/// <param name="RegularFoodCount">Number of live regular foods.</param>
public sealed record StudyPatientMedicalSummaryReadModel(
    int AllergyCount,
    int HealthConditionCount,
    int DrugProblemCount,
    int RegularDrugCount,
    int RegularSupplementCount,
    int RegularFoodCount)
{
    /// <summary>
    /// Gets an all-zero summary used for freshly provisioned patients or missing aggregates.
    /// </summary>
    public static StudyPatientMedicalSummaryReadModel Empty { get; } = new(0, 0, 0, 0, 0, 0);
}

/// <summary>
/// Doctor node on the study clinic canvas.
/// </summary>
/// <param name="UserId">Study account user identifier.</param>
/// <param name="FirstName">Profile first name.</param>
/// <param name="LastName">Profile last name.</param>
/// <param name="ProfessionCode">Optional provider profession.</param>
/// <param name="OtherProfessionLabel">Optional custom profession label.</param>
/// <param name="WorkplaceLabel">Optional workplace label.</param>
/// <param name="LoginEmail">Generated login email.</param>
/// <param name="PositionX">Horizontal canvas coordinate.</param>
/// <param name="PositionY">Vertical canvas coordinate.</param>
/// <param name="CreatedAtUtc">Member creation timestamp used for ordering.</param>
public sealed record StudyDoctorNodeReadModel(
    Guid UserId,
    string FirstName,
    string LastName,
    HealthcareProviderProfession? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string LoginEmail,
    double PositionX,
    double PositionY,
    DateTime CreatedAtUtc);

/// <summary>
/// Patient node on the study clinic canvas.
/// </summary>
/// <param name="UserId">Study account user identifier.</param>
/// <param name="FirstName">Profile first name.</param>
/// <param name="LastName">Profile last name.</param>
/// <param name="DateOfBirth">Profile date of birth.</param>
/// <param name="Gender">Profile gender.</param>
/// <param name="LoginEmail">Generated login email.</param>
/// <param name="PositionX">Horizontal canvas coordinate.</param>
/// <param name="PositionY">Vertical canvas coordinate.</param>
/// <param name="MedicalSummary">Medical record counters.</param>
/// <param name="CreatedAtUtc">Member creation timestamp used for ordering.</param>
public sealed record StudyPatientNodeReadModel(
    Guid UserId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    ProfileGender Gender,
    string LoginEmail,
    double PositionX,
    double PositionY,
    StudyPatientMedicalSummaryReadModel MedicalSummary,
    DateTime CreatedAtUtc);

/// <summary>
/// Active doctor-patient association (canvas edge).
/// </summary>
/// <param name="DoctorUserId">Doctor node user identifier.</param>
/// <param name="PatientUserId">Patient node user identifier.</param>
public sealed record StudyAssociationReadModel(
    Guid DoctorUserId,
    Guid PatientUserId);

/// <summary>
/// Complete study clinic canvas state.
/// </summary>
/// <param name="Clinic">Clinic header data.</param>
/// <param name="Doctors">Doctor nodes.</param>
/// <param name="Patients">Patient nodes.</param>
/// <param name="Associations">Active doctor-patient associations.</param>
public sealed record StudyClinicCanvasReadModel(
    StudyClinicHeaderReadModel Clinic,
    IReadOnlyList<StudyDoctorNodeReadModel> Doctors,
    IReadOnlyList<StudyPatientNodeReadModel> Patients,
    IReadOnlyList<StudyAssociationReadModel> Associations);

/// <summary>
/// Detail of a single study clinic member combining the member row, user profile, optional provider profile
/// and, for patients, the medical record counters.
/// </summary>
/// <param name="UserId">Study account user identifier.</param>
/// <param name="MemberKind">Canvas node type.</param>
/// <param name="LoginEmail">Generated login email.</param>
/// <param name="PositionX">Horizontal canvas coordinate.</param>
/// <param name="PositionY">Vertical canvas coordinate.</param>
/// <param name="FirstName">Profile first name.</param>
/// <param name="LastName">Profile last name.</param>
/// <param name="DateOfBirth">Profile date of birth.</param>
/// <param name="Gender">Profile gender.</param>
/// <param name="ProfessionCode">Optional provider profession (doctors).</param>
/// <param name="OtherProfessionLabel">Optional custom profession label (doctors).</param>
/// <param name="WorkplaceLabel">Optional workplace label (doctors).</param>
/// <param name="ShortDescription">Optional provider description (doctors).</param>
/// <param name="MedicalSummary">Medical record counters (patients only).</param>
public sealed record StudyMemberDetailReadModel(
    Guid UserId,
    StudyMemberKind MemberKind,
    string LoginEmail,
    double PositionX,
    double PositionY,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    ProfileGender Gender,
    HealthcareProviderProfession? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    StudyPatientMedicalSummaryReadModel? MedicalSummary);

/// <summary>
/// Random catalog drug form eligible for allergies, drug problems or regular drug schedules.
/// </summary>
/// <param name="DrugId">Owning drug identifier.</param>
/// <param name="DrugFormId">Drug form identifier.</param>
public sealed record StudyRandomDrugFormCandidate(
    Guid DrugId,
    Guid DrugFormId);
