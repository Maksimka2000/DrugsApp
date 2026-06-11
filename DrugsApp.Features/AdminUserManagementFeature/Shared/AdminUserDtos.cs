using System;
using System.Collections.Generic;

namespace DrugsApp.Features.AdminUserManagementFeature.Shared;

public sealed record AdminUserListItemDto(
    Guid Id,
    string FullName,
    string Email,
    string AdminRole,
    string Status,
    DateTime CreatedAtUtc,
    bool IsProtected,
    bool IsCurrentUser);

public sealed record ListAdminUsersResponse(
    IReadOnlyList<AdminUserListItemDto> Items,
    int TotalCount);

public sealed record AdminUserDetailDto(
    Guid Id,
    string FullName,
    string Email,
    string AdminRole,
    string Status,
    DateTime CreatedAtUtc,
    bool IsProtected,
    bool IsCurrentUser);

public sealed record CreateAdminUserRequest(
    string FullName,
    string Email,
    string AdminRole);

public sealed record CreateAdminUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string AdminRole,
    string Password);

public sealed record UpdateAdminUserRoleRequest(
    string AdminRole);

public sealed record ChangeAdminUserPasswordRequest(
    string NewPassword,
    string ConfirmNewPassword);

public sealed record ChangeAdminUserLoginEmailRequest(
    string Email);

public sealed record SuspendAdminUserRequest(
    bool IsSuspended);

public sealed record AdminUserSuspensionResponse(
    Guid Id,
    string Status);
