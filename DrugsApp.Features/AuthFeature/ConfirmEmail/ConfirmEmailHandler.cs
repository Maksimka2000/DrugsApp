using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AuthFeature.ConfirmEmail;

internal sealed class ConfirmEmailHandler(
    IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<ConfirmEmailCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        return await _identityAuthenticationService.ConfirmEmailAsync(
            command.UserId,
            command.Request.Token,
            cancellationToken);
    }
}
