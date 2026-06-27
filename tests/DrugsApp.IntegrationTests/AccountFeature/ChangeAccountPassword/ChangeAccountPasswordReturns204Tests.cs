using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AccountFeature.ChangeAccountPassword;
using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AccountFeature.ChangeAccountPassword;

public sealed class ChangeAccountPasswordReturns204Tests : IntegrationTestBase
{
    [Fact]
    public async Task ChangeAccountPassword_WithValidCredentials_Returns204AndAllowsLoginWithNewPassword()
    {
        string email = IntegrationAuthFlows.CreateUniqueEmail();
        const string originalPassword = "OriginalPass123!";
        const string newPassword = "ChangedPass123!";

        await IntegrationAuthFlows.RegisterAndConfirmMemberAsync(
            Client,
            EmailSender,
            email,
            originalPassword,
            CancellationToken.None);

        LoginMemberResponse login = await TestAuthHelper.LoginMemberAsync(
            Client,
            email,
            originalPassword,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, login.AccessToken);

        HttpResponseMessage changeResponse = await Client.PostAsJsonAsync(
            "/api/account/change-password",
            new ChangeAccountPasswordRequest(originalPassword, newPassword, newPassword),
            IntegrationTestJson.Options,
            CancellationToken.None);

        changeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        TestAuthHelper.ClearBearerToken(Client);

        await TestAuthHelper.LoginMemberAsync(Client, email, newPassword, CancellationToken.None);
    }
}
