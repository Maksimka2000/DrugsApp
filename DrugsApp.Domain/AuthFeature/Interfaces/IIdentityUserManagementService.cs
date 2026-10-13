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
    /// <summary>
    /// Creates a new admin user with the specified role and returns generated credentials.
    /// </summary>
    /// <param name="fullName">Full display name for the admin.</param>
    /// <param name="email">Login email for the admin.</param>
    /// <param name="adminRole">Role to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Creation result containing credentials.</returns>
    Task<ErrorOr<CreateAdminUserResult>> CreateAdminUserAsync(
        string fullName,
        string email,
        string adminRole,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a ready-to-use study account with a confirmed email in a study role, without sending any email.
    /// </summary>
    /// <param name="email">Generated login email.</param>
    /// <param name="password">Generated plain password to hash.</param>
    /// <param name="roleName">Study role to assign; must be one of <see cref="AuthRoles.StudyRoleNames"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new user identifier, or validation/conflict errors (<c>Auth.EmailAlreadyExists</c>).</returns>
    Task<ErrorOr<Guid>> CreateStudyAccountAsync(
        string email,
        string password,
        string roleName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Replaces an admin user's role and returns the updated identity snapshot.
    /// </summary>
    /// <param name="adminUserId">Identifier of the admin to update.</param>
    /// <param name="adminRole">Role to assign.</param>
    /// <param name="callerUserId">Admin performing the change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mutation result with refreshed values.</returns>
    Task<ErrorOr<AdminUserMutationResult>> ReplaceAdminRoleAsync(
        Guid adminUserId,
        string adminRole,
        Guid callerUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sets a new password for an admin user.
    /// </summary>
    /// <param name="adminUserId">Identifier of the admin.</param>
    /// <param name="newPassword">New password to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when the password is updated.</returns>
    Task<ErrorOr<Success>> SetAdminPasswordAsync(
        Guid adminUserId,
        string newPassword,
        CancellationToken cancellationToken);

    /// <summary>
    /// Changes the login email for an admin user.
    /// </summary>
    /// <param name="adminUserId">Identifier of the admin to update.</param>
    /// <param name="email">New email value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mutation result with updated email.</returns>
    Task<ErrorOr<AdminUserMutationResult>> ChangeAdminLoginEmailAsync(
        Guid adminUserId,
        string email,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes an admin user from the system.
    /// </summary>
    /// <param name="adminUserId">Identifier of the admin to remove.</param>
    /// <param name="callerUserId">Admin performing the removal.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when the user is removed.</returns>
    Task<ErrorOr<Success>> RemoveAdminUserAsync(
        Guid adminUserId,
        Guid callerUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Toggles suspension state for an admin user.
    /// </summary>
    /// <param name="adminUserId">Identifier of the admin.</param>
    /// <param name="isSuspended">Suspension flag.</param>
    /// <param name="callerUserId">Admin performing the action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Suspension result containing status.</returns>
    Task<ErrorOr<UserSuspensionResult>> SetAdminSuspensionAsync(
        Guid adminUserId,
        bool isSuspended,
        Guid callerUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Toggles suspension state for a client user.
    /// </summary>
    /// <param name="clientUserId">Identifier of the client user.</param>
    /// <param name="isSuspended">Suspension flag.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Suspension result containing status.</returns>
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
