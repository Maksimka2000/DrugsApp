using System;
using System.Collections.Generic;

namespace DrugsApp.Features.ClientUserManagementFeature.Shared;

/// <summary>
/// List item representing a client user.
/// </summary>
/// <param name="Id">User identifier.</param>
/// <param name="Email">User email.</param>
/// <param name="Roles">Assigned roles.</param>
/// <param name="Status">Account status string.</param>
/// <param name="IsEmailVerified">Indicates if email is verified.</param>
/// <param name="CreatedAtUtc">Creation timestamp in UTC.</param>
public sealed record ClientUserListItemDto(
    Guid Id,
    string Email,
    IReadOnlyList<string> Roles,
    string Status,
    bool IsEmailVerified,
    DateTime CreatedAtUtc);

/// <summary>
/// Paged response for client users listing.
/// </summary>
/// <param name="Items">Client users for the page.</param>
/// <param name="TotalCount">Total count of users.</param>
public sealed record ListClientUsersResponse(
    IReadOnlyList<ClientUserListItemDto> Items,
    int TotalCount);

/// <summary>
/// Request body for suspending/unsuspending a client user.
/// </summary>
/// <param name="IsSuspended">Target suspension state.</param>
public sealed record SuspendClientUserRequest(
    bool IsSuspended);

/// <summary>
/// Response payload for suspension updates.
/// </summary>
/// <param name="Id">User identifier.</param>
/// <param name="Status">Updated status string.</param>
public sealed record ClientUserSuspensionResponse(
    Guid Id,
    string Status);
