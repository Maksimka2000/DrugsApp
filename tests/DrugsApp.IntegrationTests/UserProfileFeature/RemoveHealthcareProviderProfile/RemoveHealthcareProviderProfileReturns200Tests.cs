using System.Net;
using System.Net.Http.Json;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.AddHealthcareProviderProfile;
using DrugsApp.Features.UserProfileFeature.RemoveHealthcareProviderProfile;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserProfileFeature.RemoveHealthcareProviderProfile;

public sealed class RemoveHealthcareProviderProfileReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task RemoveHealthcareProviderProfile_ForProviderUser_Returns200AndRemovesProfile()
    {
        AuthenticatedMember member = await UserProfileTestFlows.RegisterConfirmAndLoginAsync(
            Client,
            EmailSender,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, member.AccessToken);

        HttpResponseMessage addResponse = await Client.PostAsJsonAsync(
            "/api/healthcare-provider/profile",
            new AddHealthcareProviderProfileRequest(
                ProfessionCode: HealthcareProviderProfession.nurse,
                OtherProfessionLabel: null,
                WorkplaceLabel: "Temporary Workplace",
                ShortDescription: "To be removed."),
            IntegrationTestJson.Options,
            CancellationToken.None);

        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        AuthenticatedMember providerSession = await UserProfileTestFlows.LoginMemberAsync(
            Client,
            member.Email,
            member.Password,
            member.UserId,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, providerSession.AccessToken);

        HttpResponseMessage response = await Client.DeleteAsync(
            "/api/healthcare-provider/profile",
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        RemoveHealthcareProviderProfileResponse? payload =
            await response.Content.ReadFromJsonAsync<RemoveHealthcareProviderProfileResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.IsHealthcareProvider.Should().BeFalse();

        await UserProfilePersistenceAssertions.AssertHealthcareProviderProfileMissingAsync(
            Factory.Services,
            member.UserId,
            CancellationToken.None);
    }
}
