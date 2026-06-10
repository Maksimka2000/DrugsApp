using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AuthFeature.LoginMember;

internal sealed class LoginMemberHandler(
    IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<LoginMemberCommand, ErrorOr<LoginMemberResponse>>
{
    public async Task<ErrorOr<LoginMemberResponse>> Handle(
        LoginMemberCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<AuthTokensResult> result = await _identityAuthenticationService.LoginAsync(
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
