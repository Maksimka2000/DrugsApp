using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AuthFeature.ConfirmEmail;
using DrugsApp.Features.AuthFeature.RegisterMember;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AuthFeature.ConfirmEmail;

public sealed class ConfirmEmailActivatesAccountTests : IntegrationTestBase
{
    [Fact]
    public async Task ConfirmEmail_AfterRegistration_Returns204AndAllowsLogin()
    {
        string email = IntegrationAuthFlows.CreateUniqueEmail();
        const string password = "ConfirmEmail123!";

        RegisterMemberResponse registration = await IntegrationAuthFlows.RegisterMemberAsync(
            Client,
            email,
            password,
            CancellationToken.None);

        string? confirmationToken = EmailSender.GetLatestToken(
            registration.UserId,
            CapturedEmailKind.RegistrationConfirmation);

        confirmationToken.Should().NotBeNullOrWhiteSpace();

        HttpResponseMessage confirmResponse = await Client.PostAsJsonAsync(
            $"/api/auth/confirm-email/{registration.UserId}",
            new ConfirmEmailRequest(confirmationToken!),
            IntegrationTestJson.Options,
            CancellationToken.None);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await TestAuthHelper.LoginMemberAsync(Client, email, password, CancellationToken.None);
    }
}
