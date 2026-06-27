using System.Net.Http.Json;
using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.Features.AuthFeature.RefreshAccessToken;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AuthFeature.RefreshAccessToken;

public sealed class RefreshAccessTokenReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task RefreshAccessToken_WithValidRefreshToken_Returns200AndRotatesTokens()
    {
        const string password = "RefreshTokenTest123!";
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

        HttpResponseMessage refreshResponse = await Client.PostAsJsonAsync(
            "/api/auth/refresh",
            new RefreshAccessTokenRequest(login.RefreshToken),
            IntegrationTestJson.Options,
            CancellationToken.None);

        refreshResponse.EnsureSuccessStatusCode();

        RefreshAccessTokenResponse? refreshed = await refreshResponse.Content.ReadFromJsonAsync<RefreshAccessTokenResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        refreshed.Should().NotBeNull();
        refreshed!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBe(login.RefreshToken);
        refreshed.ExpiresInSeconds.Should().BeGreaterThan(0);
    }
}
