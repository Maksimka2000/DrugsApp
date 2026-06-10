using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.UserProfileFeature.Entities;

namespace DrugsApp.Domain.UserProfileFeature.Repositories;

/// <summary>
/// Repository contract for reproductive health profiles.
/// </summary>
public interface IClientReproductiveHealthProfileRepository
{
    Task AddAsync(ClientReproductiveHealthProfile profile, CancellationToken cancellationToken);

    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<ClientReproductiveHealthProfile?> GetTrackedByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
