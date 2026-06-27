using System.Net.Http.Json;
using DrugsApp.Features.AccountFeature.ConfirmLoginEmailChange;
using DrugsApp.Features.AccountFeature.RequestLoginEmailChange;
using DrugsApp.Features.AccountFeature.Shared;
using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.Features.AuthFeature.RegisterMember;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AccountFeature.ConfirmLoginEmailChange;

public sealed class ConfirmLoginEmailChangeFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task ConfirmLoginEmailChange_AfterRequest_Returns200AndUpdatesLoginEmail()
    {
        const string password = "EmailChangeFlow123!";
        string originalEmail = IntegrationAuthFlows.CreateUniqueEmail();
        string newEmail = IntegrationAuthFlows.CreateUniqueEmail();

        RegisterMemberResponse registration = await IntegrationAuthFlows.RegisterAndConfirmMemberAsync(
            Client,
            EmailSender,
            originalEmail,
            password,
            CancellationToken.None);

        LoginMemberResponse login = await TestAuthHelper.LoginMemberAsync(
            Client,
            originalEmail,
            password,
            CancellationToken.None);
        TestAuthHelper.ApplyBearerToken(Client, login.AccessToken);

        HttpResponseMessage requestResponse = await Client.PostAsJsonAsync(
            "/api/account/change-email",
            new RequestLoginEmailChangeRequest(newEmail, password),
            IntegrationTestJson.Options,
            CancellationToken.None);

        requestResponse.EnsureSuccessStatusCode();

        string? changeToken = EmailSender.GetLatestToken(registration.UserId, CapturedEmailKind.EmailChange);
        changeToken.Should().NotBeNullOrWhiteSpace();

        TestAuthHelper.ClearBearerToken(Client);

        HttpResponseMessage confirmResponse = await Client.PostAsJsonAsync(
            $"/api/account/confirm-email-change/{registration.UserId}",
            new ConfirmLoginEmailChangeRequest(registration.UserId, changeToken!),
            IntegrationTestJson.Options,
            CancellationToken.None);

        confirmResponse.EnsureSuccessStatusCode();

        ConfirmLoginEmailChangeResponse? payload = await confirmResponse.Content.ReadFromJsonAsync<ConfirmLoginEmailChangeResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Email.Should().Be(newEmail);
        payload.EmailChangeConfirmed.Should().BeTrue();

        await TestAuthHelper.LoginMemberAsync(Client, newEmail, password, CancellationToken.None);
    }
}
