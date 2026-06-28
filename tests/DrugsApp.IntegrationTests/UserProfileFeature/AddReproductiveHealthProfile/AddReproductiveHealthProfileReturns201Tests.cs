using System.Net;
using System.Net.Http.Json;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.AddReproductiveHealthProfile;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserProfileFeature.AddReproductiveHealthProfile;

public sealed class AddReproductiveHealthProfileReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddReproductiveHealthProfile_ForFemaleUser_Returns201AndCreatesProfile()
    {
        AuthenticatedMember member = await UserProfileTestFlows.RegisterConfirmAndLoginAsync(
            Client,
            EmailSender,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, member.AccessToken);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/user-profile/reproductive-health",
            new AddReproductiveHealthProfileRequest(
                CurrentlyPregnant: false,
                PlanningToGetPregnant: true,
                PregnancyPlanningUnknown: false,
                Menopause: false,
                ContraceptionMethodCode: ContraceptionMethodCode.oral_pill),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        ReproductiveHealthProfileResponse? payload = await response.Content.ReadFromJsonAsync<ReproductiveHealthProfileResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.PlanningToGetPregnant.Should().BeTrue();
        payload.ContraceptionMethodCode.Should().Be(ContraceptionMethodCode.oral_pill);

        await UserProfilePersistenceAssertions.AssertReproductiveProfileExistsAsync(
            Factory.Services,
            member.UserId,
            expectedCurrentlyPregnant: false,
            CancellationToken.None);
    }
}
