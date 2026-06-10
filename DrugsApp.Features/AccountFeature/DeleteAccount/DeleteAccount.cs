using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AccountFeature.DeleteAccount;

public sealed record DeleteAccountRequest(
    string CurrentPassword,
    string ConfirmationPhrase);

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
