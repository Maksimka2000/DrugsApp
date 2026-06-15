using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.PatientSupervisionFeature;

public sealed class SupervisionConnectionRepository(DrugsAppDbContext context) : ISupervisionConnectionRepository
{
    public async Task<SupervisionConnection?> GetByIdAsync(Guid connectionId, bool track, CancellationToken cancellationToken)
    {
        IQueryable<SupervisionConnection> query = context.SupervisionConnections;
        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(x => x.Id == connectionId, cancellationToken);
    }

    public async Task<SupervisionConnection?> GetByIdForPatientAsync(Guid patientUserId, Guid connectionId, bool track, CancellationToken cancellationToken)
    {
        IQueryable<SupervisionConnection> query = context.SupervisionConnections;
        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            x => x.Id == connectionId && x.PatientUserId == patientUserId,
            cancellationToken);
    }

    public async Task<SupervisionConnection?> GetByIdForProviderAsync(Guid providerUserId, Guid connectionId, bool track, CancellationToken cancellationToken)
    {
        IQueryable<SupervisionConnection> query = context.SupervisionConnections;
        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            x => x.Id == connectionId && x.HealthcareProfessionalUserId == providerUserId,
            cancellationToken);
    }

    public async Task<SupervisionConnection?> GetActiveForProviderAndPatientAsync(
        Guid providerUserId,
        Guid patientUserId,
        bool track,
        CancellationToken cancellationToken)
    {
        IQueryable<SupervisionConnection> query = context.SupervisionConnections;
        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            x => x.HealthcareProfessionalUserId == providerUserId
                 && x.PatientUserId == patientUserId
                 && x.ConnectionState == SupervisionConnectionState.Active,
            cancellationToken);
    }

    public async Task<SupervisionConnection?> GetActiveOrRemovedForPairAsync(Guid patientUserId, Guid providerUserId, CancellationToken cancellationToken)
    {
        return await context.SupervisionConnections
            .SingleOrDefaultAsync(
                x => x.PatientUserId == patientUserId && x.HealthcareProfessionalUserId == providerUserId,
                cancellationToken);
    }

    public async Task<bool> ExistsActiveForPairAsync(Guid patientUserId, Guid providerUserId, CancellationToken cancellationToken)
    {
        return await context.SupervisionConnections
            .AsNoTracking()
            .AnyAsync(
                x => x.PatientUserId == patientUserId
                     && x.HealthcareProfessionalUserId == providerUserId
                     && x.ConnectionState == SupervisionConnectionState.Active,
                cancellationToken);
    }

    public async Task AddAsync(SupervisionConnection connection, CancellationToken cancellationToken)
    {
        await context.SupervisionConnections.AddAsync(connection, cancellationToken);
    }

    public void Delete(SupervisionConnection connection)
    {
        context.SupervisionConnections.Remove(connection);
    }
}
