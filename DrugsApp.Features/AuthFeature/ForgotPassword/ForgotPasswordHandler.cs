using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AuthFeature.ForgotPassword;

internal sealed class ForgotPasswordHandler(
    IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<ForgotPasswordCommand, ErrorOr<Success>>
{
    public Task<ErrorOr<Success>> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken) =>
        _identityAuthenticationService.RequestPasswordResetAsync(
            command.Request.Email,
            cancellationToken);
}
