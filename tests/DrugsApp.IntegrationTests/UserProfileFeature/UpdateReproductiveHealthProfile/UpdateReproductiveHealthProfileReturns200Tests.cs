using System.Net;
using System.Net.Http.Json;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.AddReproductiveHealthProfile;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.Features.UserProfileFeature.UpdateReproductiveHealthProfile;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserProfileFeature.UpdateReproductiveHealthProfile;

public sealed class UpdateReproductiveHealthProfileReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateReproductiveHealthProfile_ForFemaleUser_Returns200AndUpdatesProfile()
    {
        AuthenticatedMember member = await UserProfileTestFlows.RegisterConfirmAndLoginAsync(
            Client,
            EmailSender,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, member.AccessToken);

        HttpResponseMessage addResponse = await Client.PostAsJsonAsync(
            "/api/user-profile/reproductive-health",
            new AddReproductiveHealthProfileRequest(
                CurrentlyPregnant: false,
                PlanningToGetPregnant: false,
                PregnancyPlanningUnknown: true,
                Menopause: false,
                ContraceptionMethodCode: ContraceptionMethodCode.iud),
            IntegrationTestJson.Options,
            CancellationToken.None);

        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            "/api/user-profile/reproductive-health",
            new UpdateReproductiveHealthProfileRequest(
                CurrentlyPregnant: true,
                PlanningToGetPregnant: false,
                PregnancyPlanningUnknown: false,
                Menopause: false,
                ContraceptionMethodCode: ContraceptionMethodCode.none),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        ReproductiveHealthProfileResponse? payload = await response.Content.ReadFromJsonAsync<ReproductiveHealthProfileResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.CurrentlyPregnant.Should().BeTrue();
        payload.ContraceptionMethodCode.Should().BeNull();

        await UserProfilePersistenceAssertions.AssertReproductiveProfileExistsAsync(
            Factory.Services,
            member.UserId,
            expectedCurrentlyPregnant: true,
            CancellationToken.None);
    }
}
