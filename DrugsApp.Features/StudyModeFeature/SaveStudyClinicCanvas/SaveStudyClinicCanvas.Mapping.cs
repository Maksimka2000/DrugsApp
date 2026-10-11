using DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;

namespace DrugsApp.Features.StudyModeFeature.SaveStudyClinicCanvas;

/// <summary>
/// Pure mapping helpers of SaveStudyClinicCanvas (no I/O, no business rules).
/// </summary>
public static class SaveStudyClinicCanvasMapping
{
    /// <summary>
    /// Maps the HTTP request and route values to the MediatR command.
    /// </summary>
    public static SaveStudyClinicCanvasCommand MapToCommand(this SaveStudyClinicCanvasRequest request, Guid clinicId) => new(
        ClinicId: clinicId,
        Request: request);

    /// <summary>
    /// Maps the requested associations to domain doctor-patient pairs.
    /// </summary>
    public static IReadOnlyCollection<(Guid DoctorUserId, Guid PatientUserId)> MapToAssociationPairs(
        this SaveStudyClinicCanvasRequest request) =>
        request.Associations
            .Select(association => (association.DoctorUserId, association.PatientUserId))
            .ToList();

    /// <summary>
    /// Maps the requested node positions to domain coordinates.
    /// </summary>
    public static IReadOnlyCollection<(Guid UserId, double PositionX, double PositionY)> MapToPositions(
        this SaveStudyClinicCanvasRequest request) =>
        request.Positions
            .Select(position => (position.UserId, position.PositionX, position.PositionY))
            .ToList();

    /// <summary>
    /// Maps the requested associations to supervision connection pairs.
    /// </summary>
    public static IReadOnlyCollection<SupervisionConnectionPair> MapToSupervisionPairs(
        this SaveStudyClinicCanvasRequest request) =>
        request.Associations
            .Select(association => new SupervisionConnectionPair(association.DoctorUserId, association.PatientUserId))
            .ToList();
}
