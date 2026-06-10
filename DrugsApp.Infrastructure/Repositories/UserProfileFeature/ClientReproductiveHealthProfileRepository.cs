using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.UserProfileFeature;

public sealed class ClientReproductiveHealthProfileRepository(DrugsAppDbContext _context)
    : IClientReproductiveHealthProfileRepository
{
    public async Task AddAsync(ClientReproductiveHealthProfile profile, CancellationToken cancellationToken)
    {
        await _context.ClientReproductiveHealthProfiles.AddAsync(profile, cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.ClientReproductiveHealthProfiles
            .AsNoTracking()
            .AnyAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public async Task<ClientReproductiveHealthProfile?> GetTrackedByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.ClientReproductiveHealthProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    public async Task RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        ClientReproductiveHealthProfile? profile = await _context.ClientReproductiveHealthProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (profile is null)
        {
            return;
        }

        _context.ClientReproductiveHealthProfiles.Remove(profile);
    }
}
