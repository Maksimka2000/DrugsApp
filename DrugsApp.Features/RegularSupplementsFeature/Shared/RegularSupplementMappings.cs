using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;

namespace DrugsApp.Features.RegularSupplementsFeature.Shared;

/// <summary>
/// Mapping helpers for regular supplement aggregates and DTO representations.
/// </summary>
internal static class RegularSupplementMappings
{
    public static RegularSupplementListItemDto MapToListItemDto(this ClientRegularSupplement regularSupplement) =>
        new(
            Id: regularSupplement.Id,
            Source: regularSupplement.Source,
            SupplementId: regularSupplement.SupplementId,
            OwnSupplementId: regularSupplement.OwnSupplementId,
            DisplayName: ResolveDisplayName(regularSupplement),
            Status: regularSupplement.Status,
            StartDate: regularSupplement.StartDate,
            EndDate: regularSupplement.EndDate,
            IntakeTimes: MapIntakeTimes(regularSupplement),
            DailyIntakeCount: regularSupplement.IntakeTimes.Count,
            MemberSupplements: MapMemberSupplements(regularSupplement),
            CreatedAtUtc: regularSupplement.CreatedAtUtc,
            UpdatedAtUtc: regularSupplement.UpdatedAtUtc);

    public static RegularSupplementDetailsDto MapToDetailsDto(this ClientRegularSupplement regularSupplement) =>
        new(
            Id: regularSupplement.Id,
            Source: regularSupplement.Source,
            SupplementId: regularSupplement.SupplementId,
            OwnSupplementId: regularSupplement.OwnSupplementId,
            DisplayName: ResolveDisplayName(regularSupplement),
            Status: regularSupplement.Status,
            StartDate: regularSupplement.StartDate,
            EndDate: regularSupplement.EndDate,
            IntakeTimes: MapIntakeTimes(regularSupplement),
            DailyIntakeCount: regularSupplement.IntakeTimes.Count,
            MemberSupplements: MapMemberSupplements(regularSupplement),
            CreatedAtUtc: regularSupplement.CreatedAtUtc,
            UpdatedAtUtc: regularSupplement.UpdatedAtUtc);

    public static RegularSupplementHistoryItemDto MapToHistoryItemDto(this ClientRegularSupplement regularSupplement) =>
        new(
            Id: regularSupplement.Id,
            Source: regularSupplement.Source,
            SupplementId: regularSupplement.SupplementId,
            OwnSupplementId: regularSupplement.OwnSupplementId,
            DisplayName: ResolveDisplayName(regularSupplement),
            Status: regularSupplement.Status,
            StartDate: regularSupplement.StartDate,
            EndDate: regularSupplement.EndDate,
            IntakeTimes: MapIntakeTimes(regularSupplement),
            DailyIntakeCount: regularSupplement.IntakeTimes.Count,
            MemberSupplements: MapMemberSupplements(regularSupplement),
            HistoryReason: regularSupplement.HistoryReason ?? RegularSupplementHistoryReason.ManualRemove,
            ArchivedAtUtc: regularSupplement.ArchivedAtUtc ?? regularSupplement.UpdatedAtUtc ?? regularSupplement.CreatedAtUtc,
            CreatedAtUtc: regularSupplement.CreatedAtUtc,
            UpdatedAtUtc: regularSupplement.UpdatedAtUtc);

    public static MoveRegularSupplementToHistorySummaryDto MapToMoveToHistorySummaryDto(
        this ClientRegularSupplement regularSupplement) =>
        new(
            Id: regularSupplement.Id,
            DisplayName: ResolveDisplayName(regularSupplement),
            Source: regularSupplement.Source,
            Status: regularSupplement.Status,
            HistoryReason: regularSupplement.HistoryReason ?? RegularSupplementHistoryReason.ManualRemove,
            ArchivedAtUtc: regularSupplement.ArchivedAtUtc ?? DateTime.UtcNow);

    public static OwnSupplementListItemDto MapToListItemDto(this ClientOwnSupplement ownSupplement) =>
        new(
            Id: ownSupplement.Id,
            Name: ownSupplement.Name,
            MemberSupplements: MapMemberSupplements(ownSupplement),
            MemberCount: ownSupplement.Items.Count,
            CreatedAtUtc: ownSupplement.CreatedAtUtc,
            UpdatedAtUtc: ownSupplement.UpdatedAtUtc);

    public static OwnSupplementDetailsDto MapToDetailsDto(this ClientOwnSupplement ownSupplement) =>
        new(
            Id: ownSupplement.Id,
            Name: ownSupplement.Name,
            MemberSupplements: MapMemberSupplements(ownSupplement),
            MemberCount: ownSupplement.Items.Count,
            CreatedAtUtc: ownSupplement.CreatedAtUtc,
            UpdatedAtUtc: ownSupplement.UpdatedAtUtc);

    public static string ResolveDisplayName(ClientRegularSupplement regularSupplement) =>
        regularSupplement.Source switch
        {
            RegularSupplementSource.Database => regularSupplement.Supplement?.Name ?? string.Empty,
            RegularSupplementSource.Own => regularSupplement.OwnSupplement?.Name ?? string.Empty,
            _ => string.Empty,
        };

    public static string BuildNotificationSummary(ClientRegularSupplement regularSupplement)
    {
        string displayName = ResolveDisplayName(regularSupplement);
        string sourceLabel = regularSupplement.Source == RegularSupplementSource.Own
            ? "own supplement compound"
            : "catalog supplement";

        return $"{displayName} ({sourceLabel}) regular supplement schedule has ended.";
    }

    public static RegularSupplementSource? MapSourceFromToken(string source) =>
        source.Equals("database", StringComparison.OrdinalIgnoreCase)
            ? RegularSupplementSource.Database
            : source.Equals("own", StringComparison.OrdinalIgnoreCase)
                ? RegularSupplementSource.Own
                : null;

    private static IReadOnlyCollection<MemberSupplementSummaryDto> MapMemberSupplements(
        ClientRegularSupplement regularSupplement)
    {
        if (regularSupplement.Source != RegularSupplementSource.Own || regularSupplement.OwnSupplement is null)
        {
            return Array.Empty<MemberSupplementSummaryDto>();
        }

        return MapMemberSupplements(regularSupplement.OwnSupplement);
    }

    private static IReadOnlyCollection<MemberSupplementSummaryDto> MapMemberSupplements(
        ClientOwnSupplement ownSupplement) =>
        ownSupplement.Items
            .OrderBy(item => item.SortOrder)
            .Select(item => new MemberSupplementSummaryDto(
                item.SupplementId,
                item.Supplement?.Name ?? string.Empty,
                item.SortOrder))
            .ToList();

    private static IReadOnlyCollection<string> MapIntakeTimes(ClientRegularSupplement regularSupplement) =>
        regularSupplement.IntakeTimes
            .Select(intakeTime => intakeTime.IntakeTime.ToString("HH:mm", CultureInfo.InvariantCulture))
            .OrderBy(time => time, StringComparer.Ordinal)
            .ToList();
}
