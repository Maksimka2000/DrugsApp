using System.Net.Http.Json;
using DrugsApp.Features.AccountFeature.RequestLoginEmailChange;
using DrugsApp.Features.AccountFeature.Shared;
using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.Features.AuthFeature.RegisterMember;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AccountFeature.RequestLoginEmailChange;

public sealed class RequestLoginEmailChangeReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task RequestLoginEmailChange_WithValidCredentials_Returns200AndSendsVerificationEmail()
    {
        const string password = "EmailChangeReq123!";
        string email = IntegrationAuthFlows.CreateUniqueEmail();
        string newEmail = IntegrationAuthFlows.CreateUniqueEmail();

        RegisterMemberResponse registration = await IntegrationAuthFlows.RegisterAndConfirmMemberAsync(
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

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/account/change-email",
            new RequestLoginEmailChangeRequest(newEmail, password),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        RequestLoginEmailChangeResponse? payload = await response.Content.ReadFromJsonAsync<RequestLoginEmailChangeResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Email.Should().Be(email);
        payload.PendingEmail.Should().Be(newEmail);
        payload.VerificationRequired.Should().BeTrue();

        EmailSender.GetLatestToken(registration.UserId, CapturedEmailKind.EmailChange)
            .Should()
            .NotBeNullOrWhiteSpace();
    }
}
