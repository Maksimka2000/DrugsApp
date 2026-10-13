using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;

namespace DrugsApp.Features.StudyModeFeature.Shared;

/// <summary>
/// Pure mapping helpers shared by the study mode slices (no I/O, no business rules).
/// </summary>
public static class StudyModeMappings
{
    /// <summary>
    /// Maps the domain or read model to its response DTO.
    /// </summary>
    public static StudyClinicDto MapToDto(this StudyClinic clinic, Guid callerUserId) => new(
        Id: clinic.Id,
        Name: clinic.Name,
        ScenarioDescription: clinic.ScenarioDescription,
        CreatedAtUtc: clinic.CreatedAtUtc,
        Visibility: clinic.Visibility.MapToToken(),
        IsOwner: clinic.CreatedByUserId == callerUserId);

    /// <summary>
    /// Maps the domain or read model to its response DTO.
    /// </summary>
    public static StudyClinicDto MapToDto(this StudyClinicHeaderReadModel header, Guid callerUserId) => new(
        Id: header.Id,
        Name: header.Name,
        ScenarioDescription: header.ScenarioDescription,
        CreatedAtUtc: header.CreatedAtUtc,
        Visibility: header.Visibility.MapToToken(),
        IsOwner: header.CreatedByUserId == callerUserId);

    /// <summary>
    /// Maps the domain or read model to its response DTO.
    /// </summary>
    public static StudyClinicListItemDto MapToDto(this StudyClinicSummary summary, Guid callerUserId) => new(
        Id: summary.Id,
        Name: summary.Name,
        ScenarioDescription: summary.ScenarioDescription,
        DoctorCount: summary.DoctorCount,
        PatientCount: summary.PatientCount,
        CreatedAtUtc: summary.CreatedAtUtc,
        Visibility: summary.Visibility.MapToToken(),
        IsOwner: summary.CreatedByUserId == callerUserId);

    /// <summary>
    /// Maps a visibility to its lowercase API token.
    /// </summary>
    public static string MapToToken(this StudyClinicVisibility visibility) =>
        visibility == StudyClinicVisibility.Private ? "private" : "public";

    /// <summary>
    /// Parses a visibility API token (case-insensitive); returns null for unknown values.
    /// </summary>
    public static StudyClinicVisibility? MapToVisibility(string? token) => token?.Trim().ToLowerInvariant() switch
    {
        "public" => StudyClinicVisibility.Public,
        "private" => StudyClinicVisibility.Private,
        _ => null
    };

    /// <summary>
    /// Maps the domain or read model to its response DTO.
    /// </summary>
    public static StudyPatientMedicalSummaryDto MapToDto(this StudyPatientMedicalSummaryReadModel summary) => new(
        AllergyCount: summary.AllergyCount,
        HealthConditionCount: summary.HealthConditionCount,
        DrugProblemCount: summary.DrugProblemCount,
        RegularDrugCount: summary.RegularDrugCount,
        RegularSupplementCount: summary.RegularSupplementCount,
        RegularFoodCount: summary.RegularFoodCount);

    /// <summary>
    /// Maps the domain or read model to its response DTO.
    /// </summary>
    public static StudyDoctorNodeDto MapToDto(this StudyDoctorNodeReadModel doctor) => new(
        UserId: doctor.UserId,
        FirstName: doctor.FirstName,
        LastName: doctor.LastName,
        ProfessionCode: doctor.ProfessionCode?.ToString(),
        OtherProfessionLabel: doctor.OtherProfessionLabel,
        WorkplaceLabel: doctor.WorkplaceLabel,
        LoginEmail: doctor.LoginEmail,
        PositionX: doctor.PositionX,
        PositionY: doctor.PositionY);

    /// <summary>
    /// Maps the domain or read model to its response DTO.
    /// </summary>
    public static StudyPatientNodeDto MapToDto(this StudyPatientNodeReadModel patient) => new(
        UserId: patient.UserId,
        FirstName: patient.FirstName,
        LastName: patient.LastName,
        DateOfBirth: patient.DateOfBirth,
        Gender: patient.Gender.ToString(),
        LoginEmail: patient.LoginEmail,
        PositionX: patient.PositionX,
        PositionY: patient.PositionY,
        MedicalSummary: patient.MedicalSummary.MapToDto());

    /// <summary>
    /// Maps the domain or read model to its response DTO.
    /// </summary>
    public static StudyAssociationDto MapToDto(this StudyAssociationReadModel association) => new(
        DoctorUserId: association.DoctorUserId,
        PatientUserId: association.PatientUserId);

    /// <summary>
    /// Maps a member and its decrypted password to the credentials DTO.
    /// </summary>
    public static StudyAccountCredentialsDto MapToCredentialsDto(this StudyClinicMember member, string password) => new(
        LoginEmail: member.LoginEmail,
        Password: password);

    /// <summary>
    /// Maps the optional medical summary of a member detail, defaulting to zeros.
    /// </summary>
    public static StudyPatientMedicalSummaryDto MapToMedicalSummaryDto(this StudyMemberDetailReadModel detail) =>
        (detail.MedicalSummary ?? StudyPatientMedicalSummaryReadModel.Empty).MapToDto();
}
