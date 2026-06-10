using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AccountFeature.ChangeAccountPassword;

public sealed record ChangeAccountPasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);

public sealed record ChangeAccountPasswordCommand(
    Guid UserId,
    ChangeAccountPasswordRequest Request) : IRequest<ErrorOr<Success>>;

internal sealed class ChangeAccountPasswordHandler(IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<ChangeAccountPasswordCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ChangeAccountPasswordCommand command, CancellationToken cancellationToken)
    {
        return await _identityAuthenticationService.ChangePasswordAsync(
            command.UserId,
            command.Request.CurrentPassword,
            command.Request.NewPassword,
            cancellationToken);
    }
}
