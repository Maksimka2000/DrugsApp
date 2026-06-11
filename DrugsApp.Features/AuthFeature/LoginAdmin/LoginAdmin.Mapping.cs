using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.Features.AuthFeature.LoginAdmin;

public static class LoginAdminMapping
{
    public static LoginAdminCommand MapToCommand(this LoginAdminRequest request) =>
        new(request);

    public static LoginAdminResponse MapToResponse(this AuthTokensResult result) =>
        new(
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            ExpiresInSeconds: result.ExpiresInSeconds);
}
