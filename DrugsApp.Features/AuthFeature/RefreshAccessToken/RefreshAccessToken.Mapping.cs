using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.Features.AuthFeature.RefreshAccessToken;

/// <summary>
/// Mapping helpers for refreshing access tokens.
/// </summary>
public static class RefreshAccessTokenMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Refresh token request payload.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static RefreshAccessTokenCommand MapToCommand(this RefreshAccessTokenRequest request) =>
        new(request);

    /// <summary>
    /// Maps auth tokens result to the refresh response.
    /// </summary>
    /// <param name="result">Authentication tokens.</param>
    /// <returns>Response with access and refresh tokens.</returns>
    public static RefreshAccessTokenResponse MapToResponse(this AuthTokensResult result) =>
        new(
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            ExpiresInSeconds: result.ExpiresInSeconds);
}
