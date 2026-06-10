using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;

namespace DrugsApp.Domain.UserProfileFeature.Repositories;

/// <summary>
/// Repository contract for healthcare provider profiles.
/// </summary>
public interface IHealthcareProviderProfileRepository
{
    Task AddAsync(HealthcareProviderProfile profile, CancellationToken cancellationToken);

    Task<HealthcareProviderProfile?> GetTrackedByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
