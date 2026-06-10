using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularFoodFeature.Entities;

namespace DrugsApp.Domain.RegularFoodFeature.Interfaces;

/// <summary>
/// Persistence contract for user-owned regular food schedules.
/// </summary>
public interface IClientRegularFoodRepository
{
    Task<List<ClientRegularFood>> SearchLiveByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountLiveByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        CancellationToken cancellationToken);

    Task<List<ClientRegularFood>> SearchHistoryByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountHistoryByUserIdAsync(
        Guid userId,
        string? foodNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken);

    Task<ClientRegularFood?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool track,
        CancellationToken cancellationToken);

    Task<bool> ExistsLiveForUserWithFoodIdAsync(
        Guid userId,
        Guid foodId,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task AddAsync(ClientRegularFood clientRegularFood, CancellationToken cancellationToken);

    void Remove(ClientRegularFood clientRegularFood);

    Task<int> RemoveExpiredByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<List<ClientRegularFood>> ListEndedLiveRowsAsync(
        DateOnly asOfDate,
        int skip,
        int take,
        CancellationToken cancellationToken);
}
