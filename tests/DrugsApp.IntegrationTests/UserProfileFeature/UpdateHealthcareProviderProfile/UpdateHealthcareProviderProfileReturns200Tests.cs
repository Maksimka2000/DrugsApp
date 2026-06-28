using System.Net;
using System.Net.Http.Json;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.AddHealthcareProviderProfile;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.Features.UserProfileFeature.UpdateHealthcareProviderProfile;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserProfileFeature.UpdateHealthcareProviderProfile;

public sealed class UpdateHealthcareProviderProfileReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateHealthcareProviderProfile_ForProviderUser_Returns200AndUpdatesProfile()
    {
        AuthenticatedMember member = await UserProfileTestFlows.RegisterConfirmAndLoginAsync(
            Client,
            EmailSender,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, member.AccessToken);

        HttpResponseMessage addResponse = await Client.PostAsJsonAsync(
            "/api/healthcare-provider/profile",
            new AddHealthcareProviderProfileRequest(
                ProfessionCode: HealthcareProviderProfession.pharmacist,
                OtherProfessionLabel: null,
                WorkplaceLabel: "Initial Workplace",
                ShortDescription: "Initial description."),
            IntegrationTestJson.Options,
            CancellationToken.None);

        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        const string updatedWorkplace = "Updated Integration Medical Center";
        const string updatedDescription = "Updated provider description from integration test.";

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            "/api/healthcare-provider/profile",
            new UpdateHealthcareProviderProfileRequest(
                ProfessionCode: HealthcareProviderProfession.doctor,
                OtherProfessionLabel: null,
                WorkplaceLabel: updatedWorkplace,
                ShortDescription: updatedDescription,
                OnboardingCompletedAtUtc: null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        HealthcareProviderProfileResponse? payload = await response.Content.ReadFromJsonAsync<HealthcareProviderProfileResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.ProfessionCode.Should().Be(HealthcareProviderProfession.doctor);
        payload.WorkplaceLabel.Should().Be(updatedWorkplace);
        payload.ShortDescription.Should().Be(updatedDescription);

        await UserProfilePersistenceAssertions.AssertHealthcareProviderProfileAsync(
            Factory.Services,
            member.UserId,
            HealthcareProviderProfession.doctor,
            updatedWorkplace,
            CancellationToken.None);
    }
}
