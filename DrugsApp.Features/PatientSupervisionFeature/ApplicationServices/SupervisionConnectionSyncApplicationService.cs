using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using ErrorOr;

namespace DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;

/// <summary>
/// Replaces the active supervision edge set between two user sets by removing obsolete edges, reactivating
/// previously removed edges, aligning access levels and creating missing edges in one unit of work.
/// </summary>
internal sealed class SupervisionConnectionSyncApplicationService(
    ISupervisionConnectionRepository _supervisionConnectionRepository)
    : ISupervisionConnectionSyncApplicationService
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
    /// Complete desired set of active edges.
    /// </param>
    /// <param name="accessLevel">
    /// Access level applied to every desired edge.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Success or the first domain error.
    /// </returns>
    public async Task<ErrorOr<Success>> ReplaceConnectionsAsync(
        IReadOnlyCollection<Guid> providerUserIds,
        IReadOnlyCollection<Guid> patientUserIds,
        IReadOnlyCollection<SupervisionConnectionPair> desiredPairs,
        SupervisionAccessLevel accessLevel,
        CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;

        List<SupervisionConnection> existingConnections = await _supervisionConnectionRepository.ListForPartiesAsync(
            providerUserIds,
            patientUserIds,
            cancellationToken);

        // index loaded rows by pair, preferring the active row and otherwise the most recently removed one
        Dictionary<SupervisionConnectionPair, SupervisionConnection> connectionsByPair = existingConnections
            .GroupBy(connection => new SupervisionConnectionPair(
                connection.HealthcareProfessionalUserId,
                connection.PatientUserId))
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(connection => connection.ConnectionState == SupervisionConnectionState.Active ? 0 : 1)
                    .ThenByDescending(connection => connection.RemovedAtUtc ?? DateTime.MaxValue)
                    .First());

        HashSet<SupervisionConnectionPair> desiredPairSet = desiredPairs.ToHashSet();

        // remove obsolete active edges
        foreach (SupervisionConnection obsoleteConnection in existingConnections.Where(connection =>
                     connection.ConnectionState == SupervisionConnectionState.Active
                     && !desiredPairSet.Contains(new SupervisionConnectionPair(
                         connection.HealthcareProfessionalUserId,
                         connection.PatientUserId))))
        {
            ErrorOr<Success> removeResult = obsoleteConnection.Remove(nowUtc);
            if (removeResult.IsError)
            {
                return removeResult.Errors;
            }
        }

        // add, reactivate or align desired edges
        foreach (SupervisionConnectionPair desiredPair in desiredPairSet)
        {
            ErrorOr<Success> applyResult = await ApplyDesiredPairAsync(
                desiredPair,
                connectionsByPair.GetValueOrDefault(desiredPair),
                accessLevel,
                nowUtc,
                cancellationToken);
            if (applyResult.IsError)
            {
                return applyResult.Errors;
            }
        }

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyDesiredPairAsync(
        SupervisionConnectionPair desiredPair,
        SupervisionConnection? existingConnection,
        SupervisionAccessLevel accessLevel,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        if (existingConnection is null)
        {
            ErrorOr<SupervisionConnection> createResult = SupervisionConnection.CreateOrReactivate(
                desiredPair.PatientUserId,
                desiredPair.ProviderUserId,
                accessLevel,
                nowUtc);
            if (createResult.IsError)
            {
                return createResult.Errors;
            }

            await _supervisionConnectionRepository.AddAsync(createResult.Value, cancellationToken);
            return Result.Success;
        }

        if (existingConnection.ConnectionState != SupervisionConnectionState.Active)
        {
            return existingConnection.Reactivate(accessLevel, nowUtc);
        }

        if (existingConnection.AccessLevelCode != accessLevel)
        {
            return existingConnection.UpdateAccessLevel(accessLevel, nowUtc);
        }

        return Result.Success;
    }
}
