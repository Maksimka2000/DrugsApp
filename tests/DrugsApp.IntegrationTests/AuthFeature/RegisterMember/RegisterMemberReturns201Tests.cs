using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AuthFeature.RegisterMember;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.AuthFeature.RegisterMember;

public sealed class RegisterMemberReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task RegisterMember_WithValidPayload_Returns201AndSendsConfirmationEmail()
    {
        string email = IntegrationAuthFlows.CreateUniqueEmail();
        const string password = "RegisterMember123!";

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterMemberRequest(
                Email: email,
                Password: password,
                ConfirmPassword: password,
                FirstName: "Integration",
                LastName: "Member",
                DateOfBirth: new DateOnly(1994, 2, 20),
                Gender: "male"),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        RegisterMemberResponse? payload = await response.Content.ReadFromJsonAsync<RegisterMemberResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Email.Should().Be(email);
        payload.UserId.Should().NotBe(Guid.Empty);

        EmailSender.GetLatestToken(payload.UserId, CapturedEmailKind.RegistrationConfirmation)
            .Should()
            .NotBeNullOrWhiteSpace();
    }
}
