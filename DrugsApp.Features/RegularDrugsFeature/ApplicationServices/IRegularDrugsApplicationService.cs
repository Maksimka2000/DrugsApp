using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.UpdateRegularDrug;
using ErrorOr;

namespace DrugsApp.Features.RegularDrugsFeature.ApplicationServices;

/// <summary>
/// Orchestrates regular drug schedule workflows reused by HTTP handlers and background jobs.
/// </summary>
public interface IRegularDrugsApplicationService
{
    /// <summary>
    /// Creates a regular drug schedule for the user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="request">Creation payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Created regular drug list item.</returns>
    Task<ErrorOr<RegularDrugListItemDto>> CreateAsync(
        Guid userId,
        CreateRegularDrugRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates a regular drug schedule for the user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="id">Regular drug identifier.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Updated list item.</returns>
    Task<ErrorOr<RegularDrugListItemDto>> UpdateScheduleAsync(
        Guid userId,
        Guid id,
        UpdateRegularDrugRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Moves a regular drug schedule to history with a reason.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="id">Regular drug identifier.</param>
    /// <param name="reason">History reason.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Move-to-history summary.</returns>
    Task<ErrorOr<MoveRegularDrugToHistorySummaryDto>> MoveToHistoryAsync(
        Guid userId,
        Guid id,
        RegularDrugHistoryReason reason,
        CancellationToken cancellationToken);

    /// <summary>
    /// Expires ended regular drug rows in batches.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Expire batch result.</returns>
    Task<ErrorOr<ExpireEndedRegularDrugsResult>> ExpireEndedRowsAsync(
        CancellationToken cancellationToken);
}

/// <summary>
/// Batch result for auto-expire processing.
/// </summary>
public sealed record ExpireEndedRegularDrugsResult(
    int ExpiredCount,
    int NotificationsCreated);
