using System;
using System.Collections.Generic;

namespace DrugsApp.Features.ClientUserManagementFeature.Shared;

public sealed record ClientUserListItemDto(
    Guid Id,
    string Email,
    IReadOnlyList<string> Roles,
    string Status,
    bool IsEmailVerified,
    DateTime CreatedAtUtc);

public sealed record ListClientUsersResponse(
    IReadOnlyList<ClientUserListItemDto> Items,
    int TotalCount);

public sealed record SuspendClientUserRequest(
    bool IsSuspended);

public sealed record ClientUserSuspensionResponse(
    Guid Id,
    string Status);
