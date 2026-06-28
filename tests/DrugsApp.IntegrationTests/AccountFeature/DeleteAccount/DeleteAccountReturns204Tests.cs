using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AccountFeature.DeleteAccount;
using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AccountFeature.DeleteAccount;

public sealed class DeleteAccountReturns204Tests : IntegrationTestBase
{
    [Fact]
    public async Task DeleteAccount_WithValidCredentials_Returns204AndPreventsFutureLogin()
    {
        const string password = "DeleteAccountTest123!";
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

        TestAuthHelper.ApplyBearerToken(Client, login.AccessToken);

        HttpResponseMessage deleteResponse = await Client.PostAsJsonAsync(
            "/api/account/delete",
            new DeleteAccountRequest(password, "DELETE MY ACCOUNT"),
            IntegrationTestJson.Options,
            CancellationToken.None);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        TestAuthHelper.ClearBearerToken(Client);

        HttpResponseMessage loginAfterDelete = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginMemberRequest(email, password),
            IntegrationTestJson.Options,
            CancellationToken.None);

        loginAfterDelete.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
