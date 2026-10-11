using ErrorOr;

namespace DrugsApp.Features.StudyModeFeature.Shared;

/// <summary>
/// Shared not-found errors of the study mode feature so every slice reports identical codes and metadata.
/// </summary>
public static class StudyModeErrors
{
    /// <summary>
    /// Builds the error returned when a study clinic does not exist.
    /// </summary>
    /// <param name="clinicId">
    /// Requested clinic identifier.
    /// </param>
    /// <returns>
    /// Not-found error with code <c>StudyClinic.NotFound</c> and <c>field=clinicId</c>.
    /// </returns>
    public static Error ClinicNotFound(Guid clinicId) =>
        Error.NotFound(
            code: "StudyClinic.NotFound",
            description: "Study clinic was not found.",
            metadata: new()
            {
                ["field"] = "clinicId",
                ["resourceType"] = "StudyClinic",
                ["resourceId"] = clinicId.ToString()
            });

    /// <summary>
    /// Builds the error returned when a study account is not a member of the clinic with the expected kind.
    /// </summary>
    /// <param name="userId">
    /// Requested member user identifier.
    /// </param>
    /// <param name="field">
    /// Route field that carried the identifier (<c>doctorId</c>, <c>patientId</c> or <c>userId</c>).
    /// </param>
    /// <returns>
    /// Not-found error with code <c>StudyClinicMember.NotFound</c>.
    /// </returns>
    public static Error MemberNotFound(Guid userId, string field) =>
        Error.NotFound(
            code: "StudyClinicMember.NotFound",
            description: "Study account was not found in this clinic.",
            metadata: new()
            {
                ["field"] = field,
                ["resourceType"] = "StudyClinicMember",
                ["resourceId"] = userId.ToString()
            });
}
