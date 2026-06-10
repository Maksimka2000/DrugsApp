using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Entities;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Repository contract for pending login email change entities.
/// </summary>
public interface IPendingEmailChangeRepository
{
    Task<PendingEmailChange?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<PendingEmailChange?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(PendingEmailChange pendingEmailChange, CancellationToken cancellationToken);

    Task RemoveByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    void Remove(PendingEmailChange pendingEmailChange);
}
