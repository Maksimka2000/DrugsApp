using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Features.StudyModeFeature.Shared;

namespace DrugsApp.Features.StudyModeFeature.GetStudyClinicCanvas;

/// <summary>
/// Pure mapping helpers of GetStudyClinicCanvas (no I/O, no business rules).
/// </summary>
public static class GetStudyClinicCanvasMapping
{
    /// <summary>
    /// Maps the use-case result to the HTTP response contract.
    /// </summary>
    public static StudyClinicCanvasResponse MapToResponse(this StudyClinicCanvasReadModel canvas, Guid callerUserId) => new(
        Clinic: canvas.Clinic.MapToDto(callerUserId),
        Doctors: canvas.Doctors.Select(doctor => doctor.MapToDto()).ToList(),
        Patients: canvas.Patients.Select(patient => patient.MapToDto()).ToList(),
        Associations: canvas.Associations.Select(association => association.MapToDto()).ToList());
}
