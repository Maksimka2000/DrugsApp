using System.Net.Http.Json;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserProfileFeature.CompletePatientOnboarding;

public sealed class CompletePatientOnboardingReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task CompletePatientOnboarding_ForNewPatient_Returns200AndMarksOnboardingComplete()
    {
        AuthenticatedMember member = await UserProfileTestFlows.RegisterConfirmAndLoginAsync(
            Client,
            EmailSender,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, member.AccessToken);

        HttpResponseMessage response = await Client.PostAsync(
            "/api/user-profile/complete-onboarding",
            content: null,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UserProfileResponse? payload = await response.Content.ReadFromJsonAsync<UserProfileResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Email.Should().Be(member.Email);
        payload.OnboardingCompletedAtUtc.Should().NotBeNull();

        await UserProfilePersistenceAssertions.AssertOnboardingCompletedAsync(
            Factory.Services,
            member.UserId,
            CancellationToken.None);
    }
}
