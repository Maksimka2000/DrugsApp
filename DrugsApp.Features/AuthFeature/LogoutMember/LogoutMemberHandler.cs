using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AuthFeature.LogoutMember;

internal sealed class LogoutMemberHandler(
    IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<LogoutMemberCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        LogoutMemberCommand command,
        CancellationToken cancellationToken)
    {
        return await _identityAuthenticationService.LogoutAsync(
            command.Request.RefreshToken,
            cancellationToken);
    }
}
