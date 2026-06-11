using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.UserProfileFeature;

public sealed class UserProfileRepository(DrugsAppDbContext _context) : IUserProfileRepository
{
    public async Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken)
    {
        await _context.UserProfiles.AddAsync(userProfile, cancellationToken);
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.Id == userId, cancellationToken);
    }

    public async Task<UserProfile?> GetTrackedByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(profile => profile.Id == userId, cancellationToken);
    }

    public async Task<UserProfile?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.Email.ToLower() == normalizedEmail.ToLower(), cancellationToken);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        UserProfile? profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (profile is null)
        {
            return;
        }

        _context.UserProfiles.Remove(profile);
    }
}
