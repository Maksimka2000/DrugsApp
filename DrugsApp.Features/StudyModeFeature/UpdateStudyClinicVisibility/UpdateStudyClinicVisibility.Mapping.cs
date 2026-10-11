namespace DrugsApp.Features.StudyModeFeature.UpdateStudyClinicVisibility;

/// <summary>
/// Pure mapping helpers of UpdateStudyClinicVisibility (no I/O, no business rules).
/// </summary>
public static class UpdateStudyClinicVisibilityMapping
{
    /// <summary>
    /// Maps the HTTP request and route values to the MediatR command.
    /// </summary>
    public static UpdateStudyClinicVisibilityCommand MapToCommand(
        this UpdateStudyClinicVisibilityRequest request,
        Guid clinicId,
        Guid callerUserId) => new(
        ClinicId: clinicId,
        CallerUserId: callerUserId,
        Request: request);
}
