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
    Task<ErrorOr<RegularDrugListItemDto>> CreateAsync(
        Guid userId,
        CreateRegularDrugRequest request,
        CancellationToken cancellationToken);

    Task<ErrorOr<RegularDrugListItemDto>> UpdateScheduleAsync(
        Guid userId,
        Guid id,
        UpdateRegularDrugRequest request,
        CancellationToken cancellationToken);

    Task<ErrorOr<MoveRegularDrugToHistorySummaryDto>> MoveToHistoryAsync(
        Guid userId,
        Guid id,
        RegularDrugHistoryReason reason,
        CancellationToken cancellationToken);

    Task<ErrorOr<ExpireEndedRegularDrugsResult>> ExpireEndedRowsAsync(
        CancellationToken cancellationToken);
}

/// <summary>
/// Batch result for auto-expire processing.
/// </summary>
public sealed record ExpireEndedRegularDrugsResult(
    int ExpiredCount,
    int NotificationsCreated);
