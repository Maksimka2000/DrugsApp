using ErrorOr;

namespace DrugsApp.Features.StudyModeFeature.ApplicationServices;

/// <summary>
/// Provisions ready-to-use read-only study accounts (doctor or patient) inside a study clinic without SMTP.
/// </summary>
public interface IStudyAccountProvisioningApplicationService
{
    /// <summary>
    /// Creates the Identity user, completed profiles and clinic membership of a new study account.
    /// </summary>
    /// <param name="clinicId">
    /// Target clinic identifier.
    /// </param>
    /// <param name="draft">
    /// Member kind, personal data, optional provider data and initial position.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Generated credentials and user identifier, or not found, conflict and validation errors.
    /// </returns>
    Task<ErrorOr<StudyAccountProvisioningResult>> ProvisionAsync(
        Guid clinicId,
        StudyAccountProvisioningDraft draft,
        CancellationToken cancellationToken);
}
