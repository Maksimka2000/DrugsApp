using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Identity orchestration for admin and client user management mutations.
/// </summary>
public interface IIdentityUserManagementService
{
    Task<ErrorOr<CreateAdminUserResult>> CreateAdminUserAsync(
        string fullName,
        string email,
        string adminRole,
        CancellationToken cancellationToken);

    Task<ErrorOr<AdminUserMutationResult>> ReplaceAdminRoleAsync(
        Guid adminUserId,
        string adminRole,
        Guid callerUserId,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> SetAdminPasswordAsync(
        Guid adminUserId,
        string newPassword,
        CancellationToken cancellationToken);

    Task<ErrorOr<AdminUserMutationResult>> ChangeAdminLoginEmailAsync(
        Guid adminUserId,
        string email,
        CancellationToken cancellationToken);

    Task<ErrorOr<Success>> RemoveAdminUserAsync(
        Guid adminUserId,
        Guid callerUserId,
        CancellationToken cancellationToken);

    Task<ErrorOr<UserSuspensionResult>> SetAdminSuspensionAsync(
        Guid adminUserId,
        bool isSuspended,
        Guid callerUserId,
        CancellationToken cancellationToken);

    Task<ErrorOr<UserSuspensionResult>> SetClientSuspensionAsync(
        Guid clientUserId,
        bool isSuspended,
        CancellationToken cancellationToken);
}

/// <summary>
/// Result of admin user creation including the one-time generated password.
/// </summary>
public sealed record CreateAdminUserResult(
    Guid Id,
    string FullName,
    string Email,
    string AdminRole,
    string Password);

/// <summary>
/// Result of an admin user mutation that returns refreshed identity fields.
/// </summary>
public sealed record AdminUserMutationResult(
    Guid Id,
    string FullName,
    string Email,
    string AdminRole,
    string Status,
    DateTime CreatedAtUtc);

/// <summary>
/// Result of a user suspension state change.
/// </summary>
public sealed record UserSuspensionResult(
    Guid Id,
    string Status);
