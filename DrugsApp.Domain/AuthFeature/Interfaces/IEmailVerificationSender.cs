using System;
using System.Threading;
using System.Threading.Tasks;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

/// <summary>
/// Sends Identity-related verification emails for confirmations and password resets.
/// </summary>
public interface IEmailVerificationSender
{
    /// <summary>
    /// Sends an email confirmation message for a newly registered user.
    /// </summary>
    /// <param name="userId">Identifier of the user awaiting confirmation.</param>
    /// <param name="email">Destination email address.</param>
    /// <param name="confirmationToken">Identity confirmation token to include in the message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the email is queued for delivery.</returns>
    Task SendConfirmationAsync(
        Guid userId,
        string email,
        string confirmationToken,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sends a verification message for a login email change request.
    /// </summary>
    /// <param name="userId">Identifier of the user requesting the change.</param>
    /// <param name="newEmail">New email address to verify.</param>
    /// <param name="confirmationToken">Email change confirmation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the email is queued for delivery.</returns>
    Task SendEmailChangeConfirmationAsync(
        Guid userId,
        string newEmail,
        string confirmationToken,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sends a password reset message to the specified user.
    /// </summary>
    /// <param name="userId">Identifier of the user requesting reset.</param>
    /// <param name="email">Destination email address.</param>
    /// <param name="confirmationToken">Password reset token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the email is queued for delivery.</returns>
    Task SendPasswordResetAsync(
        Guid userId,
        string email,
        string confirmationToken,
        CancellationToken cancellationToken);
}
