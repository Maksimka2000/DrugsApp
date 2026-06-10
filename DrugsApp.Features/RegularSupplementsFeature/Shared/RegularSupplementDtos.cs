using System;
using System.Collections.Generic;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;

namespace DrugsApp.Features.RegularSupplementsFeature.Shared;

/// <summary>
/// Shared projection for regular supplement list, detail, and mutation responses.
/// </summary>
public sealed record RegularSupplementListItemDto(
    Guid Id,
    RegularSupplementSource Source,
    Guid? SupplementId,
    Guid? OwnSupplementId,
    string DisplayName,
    RegularSupplementStatus Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<string> IntakeTimes,
    int DailyIntakeCount,
    IReadOnlyCollection<MemberSupplementSummaryDto> MemberSupplements,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Detail projection for regular supplement get and mutation responses.
/// </summary>
public sealed record RegularSupplementDetailsDto(
    Guid Id,
    RegularSupplementSource Source,
    Guid? SupplementId,
    Guid? OwnSupplementId,
    string DisplayName,
    RegularSupplementStatus Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<string> IntakeTimes,
    int DailyIntakeCount,
    IReadOnlyCollection<MemberSupplementSummaryDto> MemberSupplements,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Lightweight catalog member preview for own-source rows.
/// </summary>
public sealed record MemberSupplementSummaryDto(
    Guid Id,
    string Name,
    int SortOrder);

/// <summary>
/// Paged envelope for live regular supplement schedules.
/// </summary>
public sealed record ListRegularSupplementsResponse(
    IReadOnlyCollection<RegularSupplementListItemDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// History list item extending the live projection with archive metadata.
/// </summary>
public sealed record RegularSupplementHistoryItemDto(
    Guid Id,
    RegularSupplementSource Source,
    Guid? SupplementId,
    Guid? OwnSupplementId,
    string DisplayName,
    RegularSupplementStatus Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<string> IntakeTimes,
    int DailyIntakeCount,
    IReadOnlyCollection<MemberSupplementSummaryDto> MemberSupplements,
    RegularSupplementHistoryReason HistoryReason,
    DateTime ArchivedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Paged envelope for regular supplement history rows.
/// </summary>
public sealed record ListRegularSupplementHistoryResponse(
    IReadOnlyCollection<RegularSupplementHistoryItemDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>
/// Compact summary returned after manually moving a schedule to history.
/// </summary>
public sealed record MoveRegularSupplementToHistorySummaryDto(
    Guid Id,
    string DisplayName,
    RegularSupplementSource Source,
    RegularSupplementStatus Status,
    RegularSupplementHistoryReason HistoryReason,
    DateTime ArchivedAtUtc);

/// <summary>
/// Response containing the number of deleted history rows.
/// </summary>
public sealed record ClearRegularSupplementHistoryResponse(int DeletedCount);
