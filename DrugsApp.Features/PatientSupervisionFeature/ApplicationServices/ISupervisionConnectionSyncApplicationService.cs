using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using ErrorOr;

namespace DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;

/// <summary>
/// Synchronizes supervision connections between two user sets to an exact desired edge set, without supervision
/// requests or notifications; owned by the patient supervision feature that owns <c>SupervisionConnection</c>.
/// </summary>
public interface ISupervisionConnectionSyncApplicationService
{
    /// <summary>
    /// Makes the active connections between the provider and patient sets exactly match the desired pairs.
    /// </summary>
    /// <param name="providerUserIds">
    /// Every provider (study doctor) user identifier in scope.
    /// </param>
    /// <param name="patientUserIds">
    /// Every patient user identifier in scope.
    /// </param>
    /// <param name="desiredPairs">
    /// Complete desired set of active edges; pairs outside the scope sets are ignored.
    /// </param>
    /// <param name="accessLevel">
    /// Access level applied to every desired edge.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Success or the first domain error raised while removing, reactivating, updating or creating an edge.
    /// </returns>
    Task<ErrorOr<Success>> ReplaceConnectionsAsync(
        IReadOnlyCollection<Guid> providerUserIds,
        IReadOnlyCollection<Guid> patientUserIds,
        IReadOnlyCollection<SupervisionConnectionPair> desiredPairs,
        SupervisionAccessLevel accessLevel,
        CancellationToken cancellationToken);
}
