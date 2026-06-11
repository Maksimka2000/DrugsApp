using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.PatientSupervisionFeature.Repositories;

public interface ISupervisionRequestRepository
{
    Task<SupervisionRequest?> GetByIdForPatientAsync(Guid patientUserId, Guid requestId, bool track, CancellationToken cancellationToken);

    Task<SupervisionRequest?> GetByIdForProviderAsync(Guid providerUserId, Guid requestId, bool track, CancellationToken cancellationToken);

    Task<bool> ExistsPendingForPairAsync(Guid patientUserId, Guid providerUserId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SupervisionRequest>> ListExpiredPendingBatchAsync(DateTime nowUtc, int take, CancellationToken cancellationToken);

    Task AddAsync(SupervisionRequest request, CancellationToken cancellationToken);
}
