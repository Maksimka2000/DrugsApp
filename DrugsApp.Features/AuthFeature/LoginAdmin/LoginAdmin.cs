using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AuthFeature.LoginAdmin;

internal sealed class LoginAdminHandler(IIdentityAuthenticationService identityAuthenticationService)
    : IRequestHandler<LoginAdminCommand, ErrorOr<LoginAdminResponse>>
{
    public async Task<ErrorOr<LoginAdminResponse>> Handle(
        LoginAdminCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<AuthTokensResult> result = await identityAuthenticationService.LoginAdminAsync(
            command.Request.Email,
            command.Request.Password,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.MapToResponse();
    }
}
