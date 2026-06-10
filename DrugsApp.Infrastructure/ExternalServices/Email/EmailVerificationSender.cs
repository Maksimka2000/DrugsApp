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

        string confirmUrl =
            $"{clientBaseUrl}/auth/confirm-email?userId={userId:D}&token={confirmationToken}";

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
}
