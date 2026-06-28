using System.Net;
using System.Net.Http.Json;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.AddHealthcareProviderProfile;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserProfileFeature.AddHealthcareProviderProfile;

public sealed class AddHealthcareProviderProfileReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddHealthcareProviderProfile_ForClientWithoutProviderRole_Returns201AndCreatesProfile()
    {
        AuthenticatedMember member = await UserProfileTestFlows.RegisterConfirmAndLoginAsync(
            Client,
            EmailSender,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, member.AccessToken);

        const string workplace = "Integration Clinic";
        const string description = "Integration test provider profile.";

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/healthcare-provider/profile",
            new AddHealthcareProviderProfileRequest(
                ProfessionCode: HealthcareProviderProfession.pharmacist,
                OtherProfessionLabel: null,
                WorkplaceLabel: workplace,
                ShortDescription: description),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        HealthcareProviderProfileResponse? payload = await response.Content.ReadFromJsonAsync<HealthcareProviderProfileResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.ProfessionCode.Should().Be(HealthcareProviderProfession.pharmacist);
        payload.WorkplaceLabel.Should().Be(workplace);
        payload.ShortDescription.Should().Be(description);

        await UserProfilePersistenceAssertions.AssertHealthcareProviderProfileAsync(
            Factory.Services,
            member.UserId,
            HealthcareProviderProfession.pharmacist,
            workplace,
            CancellationToken.None);
    }
}
