using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.Features.AuthFeature.LoginAdmin;

/// <summary>
/// Mapping helpers for admin login.
/// </summary>
public static class LoginAdminMapping
{
    /// <summary>
    /// Maps the incoming request to a command envelope.
    /// </summary>
    /// <param name="request">Admin login request.</param>
    /// <returns>Command for MediatR handling.</returns>
    public static LoginAdminCommand MapToCommand(this LoginAdminRequest request) =>
        new(request);

    /// <summary>
    /// Maps auth tokens result to the admin login response.
    /// </summary>
    /// <param name="result">Authentication tokens.</param>
    /// <returns>Response with access and refresh tokens.</returns>
    public static LoginAdminResponse MapToResponse(this AuthTokensResult result) =>
        new(
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            ExpiresInSeconds: result.ExpiresInSeconds);
}
