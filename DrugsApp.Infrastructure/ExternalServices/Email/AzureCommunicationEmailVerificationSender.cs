using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using DrugsApp.Domain.AuthFeature.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DrugsApp.Infrastructure.ExternalServices.Email;

/// <summary>
/// Sends auth-related emails through Azure Communication Services Email (production).
/// </summary>
public sealed class AzureCommunicationEmailVerificationSender(
    EmailClient emailClient,
    IOptions<EmailSettings> emailSettings,
    ILogger<AzureCommunicationEmailVerificationSender> logger) : IEmailVerificationSender
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

    private async Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            throw new InvalidOperationException("Email:SenderEmail must be configured.");
        }

        var message = new EmailMessage(
            senderAddress: _settings.SenderEmail,
            content: new EmailContent(subject) { Html = htmlBody },
            recipients: new EmailRecipients([new EmailAddress(recipientEmail)]));

        try
        {
            EmailSendOperation operation = await emailClient.SendAsync(
                WaitUntil.Completed,
                message,
                cancellationToken);

            EmailSendResult? result = operation.Value;
            if (result is not null && result.Status != EmailSendStatus.Succeeded)
            {
                logger.LogError(
                    "Azure Communication Services email failed. Status={Status} Recipient={Recipient}",
                    result.Status,
                    recipientEmail);

                throw new InvalidOperationException(
                    $"Azure Communication Services email send failed with status {result.Status}.");
            }
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(
                ex,
                "Azure Communication Services email request failed. Recipient={Recipient}",
                recipientEmail);

            throw;
        }
    }
}
