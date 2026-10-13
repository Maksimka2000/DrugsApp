using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.AllergiesFeature;

public sealed class ClientAllergySummaryRepository(DrugsAppDbContext _context) : IClientAllergySummaryRepository
{
    public async Task<ClientAllergySummary?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.ClientAllergySummaries
            .AsNoTracking()
            .FirstOrDefaultAsync(summary => summary.UserId == userId, cancellationToken);
    }

    public async Task<ClientAllergySummary?> GetTrackedByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.ClientAllergySummaries
            .FirstOrDefaultAsync(summary => summary.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(ClientAllergySummary summary, CancellationToken cancellationToken)
    {
        await _context.ClientAllergySummaries.AddAsync(summary, cancellationToken);
    }
}
