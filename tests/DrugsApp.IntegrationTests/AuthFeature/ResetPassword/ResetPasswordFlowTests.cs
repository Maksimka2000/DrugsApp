using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AuthFeature.ForgotPassword;
using DrugsApp.Features.AuthFeature.ResetPassword;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AuthFeature.ResetPassword;

public sealed class ResetPasswordFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task ResetPassword_AfterForgotPassword_Returns204AndAllowsLoginWithNewPassword()
    {
        string email = IntegrationAuthFlows.CreateUniqueEmail();
        const string originalPassword = "OriginalPass123!";
        const string newPassword = "NewPassword123!";

        await IntegrationAuthFlows.RegisterAndConfirmMemberAsync(
            Client,
            EmailSender,
            email,
            originalPassword,
            CancellationToken.None);

        HttpResponseMessage forgotResponse = await Client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordRequest(email),
            IntegrationTestJson.Options,
            CancellationToken.None);

        forgotResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Guid userId = EmailSender.Messages
            .Last(message => message.Kind == CapturedEmailKind.PasswordReset && message.Email == email)
            .UserId;

        string? resetToken = EmailSender.GetLatestToken(userId, CapturedEmailKind.PasswordReset);
        resetToken.Should().NotBeNullOrWhiteSpace();

        HttpResponseMessage resetResponse = await Client.PostAsJsonAsync(
            $"/api/auth/reset-password/{userId}",
            new ResetPasswordRequest(resetToken!, newPassword, newPassword),
            IntegrationTestJson.Options,
            CancellationToken.None);

        resetResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await TestAuthHelper.LoginMemberAsync(Client, email, newPassword, CancellationToken.None);
    }
}
