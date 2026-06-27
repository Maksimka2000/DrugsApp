using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using DrugsApp.Domain.UserProfileFeature.Enums;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Authentication and login-email lifecycle operations for client and admin portals.
/// </summary>
public interface IIdentityAuthenticationService
{
    /// <summary>
    /// Registers a new member user and issues initial confirmation state.
    /// </summary>
    /// <param name="email">Login email to register.</param>
    /// <param name="password">Raw password to hash.</param>
    /// <param name="personalData">Profile details required at registration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration result containing user identifier and email.</returns>
    Task<ErrorOr<AuthRegistrationResult>> RegisterAsync(
        string email,
        string password,
        UserProfilePersonalData personalData,
        CancellationToken cancellationToken);

    /// <summary>
    /// Grants the healthcare provider role to an existing user.
    /// </summary>
    /// <param name="userId">Identifier of the user to elevate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when the role is assigned.</returns>
    Task<ErrorOr<Success>> AssignHealthcareProviderRoleAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes the healthcare provider role from an existing user.
    /// </summary>
    /// <param name="userId">Identifier of the user to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when the role is removed.</returns>
    Task<ErrorOr<Success>> RemoveHealthcareProviderRoleAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Authenticates a member user and issues fresh tokens.
    /// </summary>
    /// <param name="email">Login email.</param>
    /// <param name="password">Password to verify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Access and refresh tokens on success.</returns>
    Task<ErrorOr<AuthTokensResult>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    /// <summary>
    /// Authenticates an admin user and issues fresh tokens.
    /// </summary>
    /// <param name="email">Admin login email.</param>
    /// <param name="password">Password to verify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Access and refresh tokens on success.</returns>
    Task<ErrorOr<AuthTokensResult>> LoginAdminAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes an existing session using a stored refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token to validate and rotate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New access and refresh tokens.</returns>
    Task<ErrorOr<AuthTokensResult>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    /// <summary>
    /// Requests a login email change and sends verification to the new address.
    /// </summary>
    /// <param name="userId">Identifier of the requesting user.</param>
    /// <param name="newEmail">Desired new login email.</param>
    /// <param name="currentPassword">Current password used for reauthentication.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result describing pending email state.</returns>
    Task<ErrorOr<RequestLoginEmailChangeResult>> RequestEmailChangeAsync(
        Guid userId,
        string newEmail,
        string currentPassword,
        CancellationToken cancellationToken);

    /// <summary>
    /// Confirms a pending login email change using a token sent to the new address.
    /// </summary>
    /// <param name="userId">Identifier of the user confirming the change.</param>
    /// <param name="token">Email change confirmation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating whether the email change was applied.</returns>
    Task<ErrorOr<ConfirmLoginEmailChangeResult>> ConfirmEmailChangeAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken);

    /// <summary>
    /// Changes a user's password after verifying the current password.
    /// </summary>
    /// <param name="userId">Identifier of the user.</param>
    /// <param name="currentPassword">Existing password for verification.</param>
    /// <param name="newPassword">New password to set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success on password change.</returns>
    Task<ErrorOr<Success>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a client account after verifying the current password.
    /// </summary>
    /// <param name="userId">Identifier of the user.</param>
    /// <param name="currentPassword">Current password for verification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when the account is removed.</returns>
    Task<ErrorOr<Success>> DeleteAccountAsync(
        Guid userId,
        string currentPassword,
        CancellationToken cancellationToken);

    /// <summary>
    /// Performs teardown actions for a client account without password verification (admin-driven).
    /// </summary>
    /// <param name="userId">Identifier of the client account.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when the teardown completes.</returns>
    Task<ErrorOr<Success>> TeardownClientAccountAsync(
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Confirms a user's email using a confirmation token.
    /// </summary>
    /// <param name="userId">Identifier of the user.</param>
    /// <param name="token">Confirmation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success on confirmation.</returns>
    Task<ErrorOr<Success>> ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken);

    /// <summary>
    /// Resends an email confirmation to a user who has not completed verification.
    /// </summary>
    /// <param name="email">User email to target.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registration result when the confirmation is resent.</returns>
    Task<ErrorOr<AuthRegistrationResult?>> ResendConfirmationEmailAsync(
        string email,
        CancellationToken cancellationToken);

    /// <summary>
    /// Initiates a password reset flow by sending a reset token to the user.
    /// </summary>
    /// <param name="email">Login email requesting reset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when the reset token is issued.</returns>
    Task<ErrorOr<Success>> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken);

    /// <summary>
    /// Completes a password reset using a reset token.
    /// </summary>
    /// <param name="userId">Identifier of the user resetting the password.</param>
    /// <param name="token">Password reset token.</param>
    /// <param name="newPassword">New password to set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success on password reset.</returns>
    Task<ErrorOr<Success>> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken);

    /// <summary>
    /// Logs out a user by revoking the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success when logout completes.</returns>
    Task<ErrorOr<Success>> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken);
}

/// <summary>
/// Registration result containing the new user's identity.
/// </summary>
/// <param name="UserId">Newly created user identifier.</param>
/// <param name="Email">Registered login email.</param>
public sealed record AuthRegistrationResult(
    Guid UserId,
    string Email);

/// <summary>
/// Issued access and refresh tokens with lifetime metadata.
/// </summary>
/// <param name="AccessToken">JWT access token string.</param>
/// <param name="RefreshToken">Refresh token string.</param>
/// <param name="ExpiresInSeconds">Access token lifetime in seconds.</param>
public sealed record AuthTokensResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds);

/// <summary>
/// Personal data required during member registration.
/// </summary>
/// <param name="FirstName">User given name.</param>
/// <param name="LastName">User family name.</param>
/// <param name="DateOfBirth">User date of birth.</param>
/// <param name="Gender">User gender selection.</param>
public sealed record UserProfilePersonalData(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    ProfileGender Gender);

/// <summary>
/// Result produced when a login email change is requested.
/// </summary>
/// <param name="Email">Current email address.</param>
/// <param name="PendingEmail">New pending email awaiting confirmation.</param>
/// <param name="VerificationRequired">Indicates whether confirmation is required.</param>
public sealed record RequestLoginEmailChangeResult(
    string Email,
    string PendingEmail,
    bool VerificationRequired);

/// <summary>
/// Result produced after confirming a login email change.
/// </summary>
/// <param name="Email">Final email value after confirmation.</param>
/// <param name="EmailChangeConfirmed">Whether the change was applied.</param>
public sealed record ConfirmLoginEmailChangeResult(
    string Email,
    bool EmailChangeConfirmed);
