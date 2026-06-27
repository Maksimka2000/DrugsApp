using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.Features.AuthFeature.RegisterMember;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AuthFeature.LoginMember;

public sealed class LoginMemberReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task LoginMember_WithConfirmedClient_Returns200AndTokens()
    {
        const string password = "LoginMemberTest123!";
        string email = IntegrationAuthFlows.CreateUniqueEmail();

        await IntegrationAuthFlows.RegisterAndConfirmMemberAsync(
            Client,
            EmailSender,
            email,
            password,
            CancellationToken.None);

        LoginMemberResponse response = await TestAuthHelper.LoginMemberAsync(
            Client,
            email,
            password,
            CancellationToken.None);

        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
        response.ExpiresInSeconds.Should().BeGreaterThan(0);
    }
}
