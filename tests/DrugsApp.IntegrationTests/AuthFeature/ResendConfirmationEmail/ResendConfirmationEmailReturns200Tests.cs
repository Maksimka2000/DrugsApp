using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AuthFeature.RegisterMember;
using DrugsApp.Features.AuthFeature.ResendConfirmationEmail;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AuthFeature.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task ResendConfirmationEmail_ForUnconfirmedUser_Returns200AndSendsAnotherEmail()
    {
        string email = IntegrationAuthFlows.CreateUniqueEmail();
        const string password = "ResendConfirm123!";

        RegisterMemberResponse registration = await IntegrationAuthFlows.RegisterMemberAsync(
            Client,
            email,
            password,
            CancellationToken.None);

        int messagesBeforeResend = EmailSender.Messages.Count(message =>
            message.UserId == registration.UserId
            && message.Kind == CapturedEmailKind.RegistrationConfirmation);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/auth/resend-confirmation-email",
            new ResendConfirmationEmailRequest(email),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ResendConfirmationEmailResponse? payload = await response.Content.ReadFromJsonAsync<ResendConfirmationEmailResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.UserId.Should().Be(registration.UserId);
        payload.Email.Should().Be(email);

        EmailSender.Messages.Count(message =>
                message.UserId == registration.UserId
                && message.Kind == CapturedEmailKind.RegistrationConfirmation)
            .Should()
            .BeGreaterThan(messagesBeforeResend);
    }
}
