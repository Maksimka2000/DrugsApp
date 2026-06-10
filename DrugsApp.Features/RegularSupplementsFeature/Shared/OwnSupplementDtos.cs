using System;
using System.Collections.Generic;

namespace DrugsApp.Features.RegularSupplementsFeature.Shared;

/// <summary>
/// List item projection for user-owned supplement compounds.
/// </summary>
public sealed record OwnSupplementListItemDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<MemberSupplementSummaryDto> MemberSupplements,
    int MemberCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Detail projection for own supplement get and mutation responses.
/// </summary>
public sealed record OwnSupplementDetailsDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<MemberSupplementSummaryDto> MemberSupplements,
    int MemberCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Response envelope for listing own supplements.
/// </summary>
public sealed record ListOwnSupplementsResponse(
    IReadOnlyCollection<OwnSupplementListItemDto> Items);

/// <summary>
/// Response indicating whether an own supplement name is available.
/// </summary>
public sealed record CheckOwnSupplementNameAvailabilityResponse(
    bool IsAvailable,
    Guid? ExistingId);

/// <summary>
/// Response returned after cascade-deleting an own supplement.
/// </summary>
public sealed record DeleteOwnSupplementResponse(
    int DeletedRegularSupplementCount,
    int DeletedHistoryCount);
