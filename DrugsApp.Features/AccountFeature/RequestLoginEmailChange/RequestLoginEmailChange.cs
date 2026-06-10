using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Features.AccountFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AccountFeature.RequestLoginEmailChange;

public sealed record RequestLoginEmailChangeRequest(
    string NewEmail,
    string CurrentPassword);

public sealed record RequestLoginEmailChangeCommand(
    Guid UserId,
    RequestLoginEmailChangeRequest Request) : IRequest<ErrorOr<RequestLoginEmailChangeResponse>>;

internal sealed class RequestLoginEmailChangeHandler(IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<RequestLoginEmailChangeCommand, ErrorOr<RequestLoginEmailChangeResponse>>
{
    public async Task<ErrorOr<RequestLoginEmailChangeResponse>> Handle(RequestLoginEmailChangeCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<RequestLoginEmailChangeResult> result = await _identityAuthenticationService.RequestEmailChangeAsync(
            command.UserId,
            command.Request.NewEmail,
            command.Request.CurrentPassword,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return new RequestLoginEmailChangeResponse(
            Email: result.Value.Email,
            PendingEmail: result.Value.PendingEmail,
            VerificationRequired: result.Value.VerificationRequired);
    }
}
