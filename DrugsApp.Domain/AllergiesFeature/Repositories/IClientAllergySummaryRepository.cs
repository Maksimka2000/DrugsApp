using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;

namespace DrugsApp.Domain.AllergiesFeature.Repositories;

/// <summary>
/// Persistence contract for per-user allergy summary answers.
/// </summary>
public interface IClientAllergySummaryRepository
{
    Task<ClientAllergySummary?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the allergy summary of a user with change tracking so updated answers are saved by the transaction.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tracked summary, or <c>null</c> when none exists.</returns>
    Task<ClientAllergySummary?> GetTrackedByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(ClientAllergySummary summary, CancellationToken cancellationToken);
}
