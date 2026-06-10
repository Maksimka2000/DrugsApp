using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;

namespace DrugsApp.Domain.UserProfileFeature.Repositories;

/// <summary>
/// Repository contract for user profile aggregates.
/// </summary>
public interface IUserProfileRepository
{
    Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken);

    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserProfile?> GetTrackedByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
