namespace DrugsApp.Features.StudyModeFeature.Shared;

/// <summary>
/// Paged envelope of study clinic search results.
/// </summary>
/// <param name="Items">Page items.</param>
/// <param name="TotalCount">Total matches for the filter.</param>
public sealed record StudyClinicListResponse(
    IReadOnlyList<StudyClinicListItemDto> Items,
    int TotalCount);

/// <summary>
/// Study clinic search row.
/// </summary>
/// <param name="Id">Clinic identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="ScenarioDescription">Optional scenario text.</param>
/// <param name="DoctorCount">Number of doctor members.</param>
/// <param name="PatientCount">Number of patient members.</param>
/// <param name="CreatedAtUtc">Creation timestamp.</param>
/// <param name="Visibility"><c>public</c> (every study mode admin) or <c>private</c> (creator only).</param>
/// <param name="IsOwner">True when the caller created the clinic and may change its visibility.</param>
public sealed record StudyClinicListItemDto(
    Guid Id,
    string Name,
    string? ScenarioDescription,
    int DoctorCount,
    int PatientCount,
    DateTime CreatedAtUtc,
    string Visibility,
    bool IsOwner);

/// <summary>
/// Study clinic header data.
/// </summary>
/// <param name="Id">Clinic identifier.</param>
/// <param name="Name">Normalized display name.</param>
/// <param name="ScenarioDescription">Optional scenario text.</param>
/// <param name="CreatedAtUtc">Creation timestamp.</param>
/// <param name="Visibility"><c>public</c> (every study mode admin) or <c>private</c> (creator only).</param>
/// <param name="IsOwner">True when the caller created the clinic and may change its visibility.</param>
public sealed record StudyClinicDto(
    Guid Id,
    string Name,
    string? ScenarioDescription,
    DateTime CreatedAtUtc,
    string Visibility,
    bool IsOwner);

/// <summary>
/// Complete study clinic canvas state.
/// </summary>
/// <param name="Clinic">Clinic header data.</param>
/// <param name="Doctors">Doctor nodes ordered by creation time.</param>
/// <param name="Patients">Patient nodes ordered by creation time.</param>
/// <param name="Associations">Active edges ordered by doctor id, then patient id.</param>
public sealed record StudyClinicCanvasResponse(
    StudyClinicDto Clinic,
    IReadOnlyList<StudyDoctorNodeDto> Doctors,
    IReadOnlyList<StudyPatientNodeDto> Patients,
    IReadOnlyList<StudyAssociationDto> Associations);

/// <summary>
/// Doctor card on the canvas.
/// </summary>
/// <param name="UserId">Node identifier (study account user id).</param>
/// <param name="FirstName">Profile first name.</param>
/// <param name="LastName">Profile last name.</param>
/// <param name="ProfessionCode">Provider profession code.</param>
/// <param name="OtherProfessionLabel">Custom profession label.</param>
/// <param name="WorkplaceLabel">Workplace label.</param>
/// <param name="LoginEmail">Generated login email (never the password).</param>
/// <param name="PositionX">Horizontal canvas coordinate.</param>
/// <param name="PositionY">Vertical canvas coordinate.</param>
public sealed record StudyDoctorNodeDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string? ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string LoginEmail,
    double PositionX,
    double PositionY);

/// <summary>
/// Patient card on the canvas.
/// </summary>
/// <param name="UserId">Node identifier (study account user id).</param>
/// <param name="FirstName">Profile first name.</param>
/// <param name="LastName">Profile last name.</param>
/// <param name="DateOfBirth">Profile date of birth.</param>
/// <param name="Gender">Profile gender.</param>
/// <param name="LoginEmail">Generated login email (never the password).</param>
/// <param name="PositionX">Horizontal canvas coordinate.</param>
/// <param name="PositionY">Vertical canvas coordinate.</param>
/// <param name="MedicalSummary">Medical record counters for the card chips.</param>
public sealed record StudyPatientNodeDto(
    Guid UserId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string LoginEmail,
    double PositionX,
    double PositionY,
    StudyPatientMedicalSummaryDto MedicalSummary);

/// <summary>
/// Medical record counters of a study patient.
/// </summary>
/// <param name="AllergyCount">Sum of drug, supplement, food and other allergies.</param>
/// <param name="HealthConditionCount">User health conditions.</param>
/// <param name="DrugProblemCount">Drug problems.</param>
/// <param name="RegularDrugCount">Live regular drugs.</param>
/// <param name="RegularSupplementCount">Live regular supplements.</param>
/// <param name="RegularFoodCount">Live regular foods.</param>
public sealed record StudyPatientMedicalSummaryDto(
    int AllergyCount,
    int HealthConditionCount,
    int DrugProblemCount,
    int RegularDrugCount,
    int RegularSupplementCount,
    int RegularFoodCount);

/// <summary>
/// Canvas edge between a doctor and a patient.
/// </summary>
/// <param name="DoctorUserId">Doctor node identifier.</param>
/// <param name="PatientUserId">Patient node identifier.</param>
public sealed record StudyAssociationDto(
    Guid DoctorUserId,
    Guid PatientUserId);
