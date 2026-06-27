using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.Features.AuthFeature.LogoutMember;
using DrugsApp.Features.AuthFeature.RefreshAccessToken;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AuthFeature.LogoutMember;

public sealed class LogoutMemberRevokesRefreshTokenTests : IntegrationTestBase
{
    [Fact]
    public async Task LogoutMember_AfterLogin_Returns204AndInvalidatesRefreshToken()
    {
        const string password = "LogoutMemberTest123!";
        string email = IntegrationAuthFlows.CreateUniqueEmail();

        await IntegrationAuthFlows.RegisterAndConfirmMemberAsync(
            Client,
            EmailSender,
            email,
            password,
            CancellationToken.None);

        LoginMemberResponse login = await TestAuthHelper.LoginMemberAsync(
            Client,
            email,
            password,
            CancellationToken.None);

        HttpResponseMessage logoutResponse = await Client.PostAsJsonAsync(
            "/api/auth/logout",
            new LogoutMemberRequest(login.RefreshToken),
            IntegrationTestJson.Options,
            CancellationToken.None);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage refreshResponse = await Client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshAccessTokenRequest(login.RefreshToken),
            IntegrationTestJson.Options,
            CancellationToken.None);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
