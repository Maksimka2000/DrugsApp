using DrugsApp.Domain.AuthFeature.Interfaces;

namespace DrugsApp.Features.AuthFeature.LoginMember;

public static class LoginMemberMapping
{
    public static LoginMemberCommand MapToCommand(this LoginMemberRequest request) =>
        new(request);

    public static LoginMemberResponse MapToResponse(this AuthTokensResult result) =>
        new(
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            ExpiresInSeconds: result.ExpiresInSeconds);
}
