using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AccountFeature.DeleteAccount;

/// <summary>
/// Request payload for deleting the current user's account.
/// </summary>
public sealed record DeleteAccountRequest
{
    /// <summary>
    /// Current password used to authorize deletion.
    /// </summary>
    public string CurrentPassword { get; init; }

    /// <summary>
    /// Explicit confirmation phrase required to proceed.
    /// </summary>
    public string ConfirmationPhrase { get; init; }

    /// <summary>
    /// Creates an account deletion request with confirmation details.
    /// </summary>
    /// <param name="currentPassword">Current password used for verification.</param>
    /// <param name="confirmationPhrase">Deletion confirmation phrase.</param>
    public DeleteAccountRequest(string currentPassword, string confirmationPhrase)
    {
        CurrentPassword = currentPassword;
        ConfirmationPhrase = confirmationPhrase;
    }
}

/// <summary>
/// Command envelope for deleting a user's account.
/// </summary>
public sealed record DeleteAccountCommand(
    Guid UserId,
    DeleteAccountRequest Request) : IRequest<ErrorOr<Success>>;

internal sealed class DeleteAccountHandler(IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<DeleteAccountCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
        return await _identityAuthenticationService.DeleteAccountAsync(
            command.UserId,
            command.Request.CurrentPassword,
            cancellationToken);
    }
}
