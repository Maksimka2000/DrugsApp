using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Interfaces;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AuthFeature.RefreshAccessToken;

internal sealed class RefreshAccessTokenHandler(
    IIdentityAuthenticationService _identityAuthenticationService)
    : IRequestHandler<RefreshAccessTokenCommand, ErrorOr<RefreshAccessTokenResponse>>
{
    public async Task<ErrorOr<RefreshAccessTokenResponse>> Handle(
        RefreshAccessTokenCommand command,
        CancellationToken cancellationToken)
    {
        ErrorOr<AuthTokensResult> result = await _identityAuthenticationService.RefreshAsync(
            command.Request.RefreshToken,
            cancellationToken);

        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.MapToResponse();
    }
}
