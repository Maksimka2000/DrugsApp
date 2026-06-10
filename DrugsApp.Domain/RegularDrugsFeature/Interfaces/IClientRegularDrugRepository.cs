using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularDrugsFeature.Entities;

namespace DrugsApp.Domain.RegularDrugsFeature.Interfaces;

/// <summary>
/// Persistence contract for user-owned regular drug schedules.
/// </summary>
public interface IClientRegularDrugRepository
{
    Task<List<ClientRegularDrug>> SearchLiveByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountLiveByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        CancellationToken cancellationToken);

    Task<List<ClientRegularDrug>> SearchHistoryByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountHistoryByUserIdAsync(
        Guid userId,
        string? drugNameQuery,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken);

    Task<ClientRegularDrug?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        bool track,
        CancellationToken cancellationToken);

    Task<bool> ExistsLiveForUserWithDrugIdAsync(
        Guid userId,
        Guid drugId,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task AddAsync(ClientRegularDrug clientRegularDrug, CancellationToken cancellationToken);

    void Remove(ClientRegularDrug clientRegularDrug);

    Task<int> DeleteExpiredByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<List<ClientRegularDrug>> FindEndedLiveRowsAsync(
        DateOnly asOfDate,
        int take,
        CancellationToken cancellationToken);
}
