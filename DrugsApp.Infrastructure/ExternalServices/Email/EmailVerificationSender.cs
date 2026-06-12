using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using FluentEmail.Core;
using Microsoft.Extensions.Options;

namespace DrugsApp.Infrastructure.ExternalServices.Email;

public sealed class EmailVerificationSender(
    IFluentEmailFactory _emailFactory,
    IOptions<EmailSettings> _emailSettings) : IEmailVerificationSender
{
    public async Task SendConfirmationAsync(
        Guid userId,
        string email,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        EmailSettings settings = _emailSettings.Value;

        string clientBaseUrl = settings.ClientBaseUrl.TrimEnd('/');

        string encodedToken = Uri.EscapeDataString(confirmationToken);

        string confirmUrl =
            $"{clientBaseUrl}/auth/confirm-email?userId={userId:D}&token={encodedToken}";

        string htmlBody =
            $"""
             <p>To verify your email address, click this link:</p>
             <p><a href="{confirmUrl}">Confirm email</a></p>
             """;

        await _emailFactory
            .Create()
            .To(email)
            .Subject("Confirm your email")
            .Body(htmlBody, isHtml: true)
            .SendAsync(cancellationToken);
    }

    public async Task SendEmailChangeConfirmationAsync(
        Guid userId,
        string newEmail,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        EmailSettings settings = _emailSettings.Value;

        string clientBaseUrl = settings.ClientBaseUrl.TrimEnd('/');

        string encodedToken = Uri.EscapeDataString(confirmationToken);

        string confirmUrl =
            $"{clientBaseUrl}/account/confirm-email-change?userId={userId:D}&token={encodedToken}";

        string htmlBody =
            $"""
             <p>To confirm your new login email, click this link:</p>
             <p><a href="{confirmUrl}">Confirm email change</a></p>
             """;

        await _emailFactory
            .Create()
            .To(newEmail)
            .Subject("Confirm your new email")
            .Body(htmlBody, isHtml: true)
            .SendAsync(cancellationToken);
    }

    public async Task SendPasswordResetAsync(
        Guid userId,
        string email,
        string confirmationToken,
        CancellationToken cancellationToken)
    {
        EmailSettings settings = _emailSettings.Value;

        string clientBaseUrl = settings.ClientBaseUrl.TrimEnd('/');

        string encodedToken = Uri.EscapeDataString(confirmationToken);

        string resetUrl =
            $"{clientBaseUrl}/auth/reset-password?userId={userId:D}&token={encodedToken}";

        string htmlBody =
            $"""
             <p>To reset your password, click this link:</p>
             <p><a href="{resetUrl}">Reset password</a></p>
             """;

        await _emailFactory
            .Create()
            .To(email)
            .Subject("Reset your password")
            .Body(htmlBody, isHtml: true)
            .SendAsync(cancellationToken);
    }
}
