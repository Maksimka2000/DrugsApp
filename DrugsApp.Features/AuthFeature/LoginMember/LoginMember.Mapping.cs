using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.Features.AuthFeature.LoginMember;

/// <summary>
/// Mapping helpers for member login.
/// </summary>
public static class LoginMemberMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Member login request.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static LoginMemberCommand MapToCommand(this LoginMemberRequest request) =>
        new(request);

    /// <summary>
    /// Maps auth tokens result to the member login response.
    /// </summary>
    /// <param name="result">Authentication tokens.</param>
    /// <returns>Response with access and refresh tokens.</returns>
    public static LoginMemberResponse MapToResponse(this AuthTokensResult result) =>
        new(
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            ExpiresInSeconds: result.ExpiresInSeconds);
}
