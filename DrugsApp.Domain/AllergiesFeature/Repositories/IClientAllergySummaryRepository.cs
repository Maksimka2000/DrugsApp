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

    Task AddAsync(ClientAllergySummary summary, CancellationToken cancellationToken);
}
