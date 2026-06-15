using DrugsApp.Domain.PatientSupervisionFeature.Entities;

namespace DrugsApp.Domain.PatientSupervisionFeature.Repositories;

public interface ISupervisionConnectionRepository
{
    Task<SupervisionConnection?> GetByIdAsync(Guid connectionId, bool track, CancellationToken cancellationToken);

    Task<SupervisionConnection?> GetByIdForPatientAsync(Guid patientUserId, Guid connectionId, bool track, CancellationToken cancellationToken);

    Task<SupervisionConnection?> GetByIdForProviderAsync(Guid providerUserId, Guid connectionId, bool track, CancellationToken cancellationToken);

    Task<SupervisionConnection?> GetActiveForProviderAndPatientAsync(
        Guid providerUserId,
        Guid patientUserId,
        bool track,
        CancellationToken cancellationToken);

    Task<SupervisionConnection?> GetActiveOrRemovedForPairAsync(Guid patientUserId, Guid providerUserId, CancellationToken cancellationToken);

    Task<bool> ExistsActiveForPairAsync(Guid patientUserId, Guid providerUserId, CancellationToken cancellationToken);

    Task AddAsync(SupervisionConnection connection, CancellationToken cancellationToken);

    void Delete(SupervisionConnection connection);
}
