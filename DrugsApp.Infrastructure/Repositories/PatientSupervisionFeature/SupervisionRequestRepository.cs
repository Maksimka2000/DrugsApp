using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.PatientSupervisionFeature;

public sealed class SupervisionRequestRepository(DrugsAppDbContext context) : ISupervisionRequestRepository
{
    public async Task<SupervisionRequest?> GetByIdAsync(Guid requestId, bool track, CancellationToken cancellationToken)
    {
        IQueryable<SupervisionRequest> query = context.SupervisionRequests;
        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query
            .Where(x => x.Id == requestId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<SupervisionRequest?> GetByIdForPatientAsync(Guid patientUserId, Guid requestId, bool track, CancellationToken cancellationToken)
    {
        IQueryable<SupervisionRequest> query = context.SupervisionRequests;
        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query
            .Where(x => x.Id == requestId && x.PatientUserId == patientUserId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<SupervisionRequest?> GetByIdForProviderAsync(Guid providerUserId, Guid requestId, bool track, CancellationToken cancellationToken)
    {
        IQueryable<SupervisionRequest> query = context.SupervisionRequests;
        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query
            .Where(x => x.Id == requestId && x.HealthcareProfessionalUserId == providerUserId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsPendingForPairAsync(Guid patientUserId, Guid providerUserId, CancellationToken cancellationToken)
    {
        return await context.SupervisionRequests
            .AsNoTracking()
            .AnyAsync(
                x => x.PatientUserId == patientUserId
                     && x.HealthcareProfessionalUserId == providerUserId
                     && x.RequestState == SupervisionRequestState.Pending,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<SupervisionRequest>> ListExpiredPendingBatchAsync(DateTime nowUtc, int take, CancellationToken cancellationToken)
    {
        return await context.SupervisionRequests
            .Where(x => x.RequestState == SupervisionRequestState.Pending && x.ExpiresAtUtc <= nowUtc)
            .OrderBy(x => x.ExpiresAtUtc)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SupervisionRequest request, CancellationToken cancellationToken)
    {
        await context.SupervisionRequests.AddAsync(request, cancellationToken);
    }

    public void Delete(SupervisionRequest request)
    {
        context.SupervisionRequests.Remove(request);
    }
}
