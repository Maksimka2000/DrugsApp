using System;
using System.Threading;
using System.Threading.Tasks;

namespace DrugsApp.Domain.AuthFeature.Interfaces;

public interface IEmailVerificationSender
{
    Task SendConfirmationAsync(
        Guid userId,
        string email,
        string confirmationToken,
        CancellationToken cancellationToken);

    Task SendEmailChangeConfirmationAsync(
        Guid userId,
        string newEmail,
        string confirmationToken,
        CancellationToken cancellationToken);
}
