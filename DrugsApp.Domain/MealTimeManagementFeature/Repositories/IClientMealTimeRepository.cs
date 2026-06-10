using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;

namespace DrugsApp.Domain.MealTimeManagementFeature.Repositories;

/// <summary>
/// Persistence contract for user-owned meal time records.
/// </summary>
public interface IClientMealTimeRepository
{
    Task<List<ClientMealTime>> ListByUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<ClientMealTime?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool tracking,
        CancellationToken cancellationToken);

    Task<bool> ExistsWithNormalizedMealNameAsync(
        Guid userId,
        string normalizedMealName,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task<bool> ExistsWithIntakeTimeAsync(
        Guid userId,
        TimeOnly intakeTime,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task AddAsync(ClientMealTime mealTime, CancellationToken cancellationToken);

    void Remove(ClientMealTime mealTime);
}
