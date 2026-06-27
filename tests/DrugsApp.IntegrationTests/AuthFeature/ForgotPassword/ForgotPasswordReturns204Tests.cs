using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AuthFeature.ForgotPassword;
using DrugsApp.Features.AuthFeature.RegisterMember;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AuthFeature.ForgotPassword;

public sealed class ForgotPasswordReturns204Tests : IntegrationTestBase
{
    [Fact]
    public async Task ForgotPassword_WithExistingClient_Returns204AndCapturesResetToken()
    {
        const string password = "ForgotPassword123!";
        string email = IntegrationAuthFlows.CreateUniqueEmail();

        RegisterMemberResponse registration = await IntegrationAuthFlows.RegisterAndConfirmMemberAsync(
            Client,
            EmailSender,
            email,
            password,
            CancellationToken.None);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordRequest(email),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        EmailSender.GetLatestToken(registration.UserId, CapturedEmailKind.PasswordReset)
            .Should()
            .NotBeNullOrWhiteSpace();
    }
}
