using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AccountFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AccountFeature.ConfirmLoginEmailChange;

public sealed record ConfirmLoginEmailChangeRequest(
    Guid UserId,
    string Token);

public sealed record ConfirmLoginEmailChangeCommand(
    Guid UserId,
    string Token) : IRequest<ErrorOr<ConfirmLoginEmailChangeResponse>>;

internal sealed class ConfirmLoginEmailChangeHandler(IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<ConfirmLoginEmailChangeCommand, ErrorOr<ConfirmLoginEmailChangeResponse>>
{
    public async Task<ErrorOr<ConfirmLoginEmailChangeResponse>> Handle(ConfirmLoginEmailChangeCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<ConfirmLoginEmailChangeResult> result = await _identityAuthenticationService.ConfirmEmailChangeAsync(
            command.UserId,
            command.Token,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return new ConfirmLoginEmailChangeResponse(
            Email: result.Value.Email,
            EmailChangeConfirmed: result.Value.EmailChangeConfirmed);
    }
}
