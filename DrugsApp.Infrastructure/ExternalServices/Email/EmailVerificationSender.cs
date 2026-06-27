using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using FluentEmail.Core;
using Microsoft.Extensions.Options;

namespace DrugsApp.Infrastructure.ExternalServices.Email;

/// <summary>
/// Sends auth-related emails through SMTP (Papercut in local development).
/// </summary>
public sealed class EmailVerificationSender(
    IFluentEmailFactory emailFactory,
    IOptions<EmailSettings> emailSettings) : IEmailVerificationSender
{
    private readonly EmailSettings _settings = emailSettings.Value;

    /// <summary>
    /// Sends an email confirmation message for a newly registered user.
    /// </summary>
    /// <param name="userId">User identifier for correlation.</param>
    /// <param name="email">Destination email address.</param>
    /// <param name="confirmationToken">Identity email confirmation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendConfirmationAsync(
        Guid userId,
        string email,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        (string subject, string htmlBody, _) = EmailVerificationContent.BuildRegistrationConfirmation(
            _settings,
            userId,
            confirmationToken);

        await SendAsync(email, subject, htmlBody, cancellationToken);
    }

    /// <summary>
    /// Sends a login email change confirmation message.
    /// </summary>
    /// <param name="userId">User identifier for correlation.</param>
    /// <param name="newEmail">New email address to verify.</param>
    /// <param name="confirmationToken">Email change confirmation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendEmailChangeConfirmationAsync(
        Guid userId,
        string newEmail,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        (string subject, string htmlBody, _) = EmailVerificationContent.BuildEmailChangeConfirmation(
            _settings,
            userId,
            confirmationToken);

        await SendAsync(newEmail, subject, htmlBody, cancellationToken);
    }

    /// <summary>
    /// Sends a password reset email.
    /// </summary>
    /// <param name="userId">User identifier for correlation.</param>
    /// <param name="email">Destination email address.</param>
    /// <param name="confirmationToken">Password reset token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendPasswordResetAsync(
        Guid userId,
        string email,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        (string subject, string htmlBody, _) = EmailVerificationContent.BuildPasswordReset(
            _settings,
            userId,
            confirmationToken);

        await SendAsync(email, subject, htmlBody, cancellationToken);
    }

    private Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken) =>
        emailFactory
            .Create()
            .To(recipientEmail)
            .Subject(subject)
            .Body(htmlBody, isHtml: true)
            .SendAsync(cancellationToken);
}
