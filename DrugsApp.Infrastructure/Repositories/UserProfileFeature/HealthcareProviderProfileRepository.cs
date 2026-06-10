using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.UserProfileFeature;

public sealed class HealthcareProviderProfileRepository(DrugsAppDbContext _context)
    : IHealthcareProviderProfileRepository
{
    public async Task AddAsync(HealthcareProviderProfile profile, CancellationToken cancellationToken)
    {
        await _context.HealthcareProviderProfiles.AddAsync(profile, cancellationToken);
    }

    public async Task<HealthcareProviderProfile?> GetTrackedByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.HealthcareProviderProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public async Task RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        HealthcareProviderProfile? profile = await _context.HealthcareProviderProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (profile is null)
        {
            return;
        }

        _context.HealthcareProviderProfiles.Remove(profile);
    }
}
