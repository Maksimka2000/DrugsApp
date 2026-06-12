using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AuthFeature.ResetPassword;

internal sealed class ResetPasswordHandler(
    IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<ResetPasswordCommand, ErrorOr<Success>>
{
    public Task<ErrorOr<Success>> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken) =>
        _identityAuthenticationService.ResetPasswordAsync(
            command.UserId,
            command.Request.Token,
            command.Request.NewPassword,
            cancellationToken);
}
