using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.AuthFeature.ConfirmEmail;
using DrugsApp.Features.AuthFeature.RegisterMember;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.Common;

internal static class IntegrationAuthFlows
{
    internal static async Task<RegisterMemberResponse> RegisterAndConfirmMemberAsync(
        HttpClient client,
        CapturingEmailVerificationSender emailSender,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        RegisterMemberResponse registration = await RegisterMemberAsync(
            client,
            email,
            password,
            cancellationToken);

        string? confirmationToken = emailSender.GetLatestToken(
            registration.UserId,
            CapturedEmailKind.RegistrationConfirmation);

        confirmationToken.Should().NotBeNullOrWhiteSpace();

        HttpResponseMessage confirmResponse = await client.PostAsJsonAsync(
            $"/api/auth/confirm-email/{registration.UserId}",
            new ConfirmEmailRequest(confirmationToken!),
            IntegrationTestJson.Options,
            cancellationToken);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        return registration;
    }

    internal static async Task<RegisterMemberResponse> RegisterMemberAsync(
        HttpClient client,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterMemberRequest(
                Email: email,
                Password: password,
                ConfirmPassword: password,
                FirstName: "Integration",
                LastName: "Tester",
                DateOfBirth: new DateOnly(1995, 6, 15),
                Gender: "female"),
            IntegrationTestJson.Options,
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        RegisterMemberResponse? payload = await response.Content.ReadFromJsonAsync<RegisterMemberResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Register response body was empty.");
    }

    internal static string CreateUniqueEmail() => $"integration-{Guid.NewGuid():N}@test.local";
}
