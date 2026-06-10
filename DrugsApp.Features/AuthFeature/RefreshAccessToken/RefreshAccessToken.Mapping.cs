using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.Features.AuthFeature.RefreshAccessToken;

public static class RefreshAccessTokenMapping
{
    public static RefreshAccessTokenCommand MapToCommand(this RefreshAccessTokenRequest request) =>
        new(request);

    public static RefreshAccessTokenResponse MapToResponse(this AuthTokensResult result) =>
        new(
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            ExpiresInSeconds: result.ExpiresInSeconds);
}
