using System.Net;
using System.Net.Http.Json;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.AddReproductiveHealthProfile;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserProfileFeature.RemoveReproductiveHealthProfile;

public sealed class RemoveReproductiveHealthProfileReturns204Tests : IntegrationTestBase
{
    [Fact]
    public async Task RemoveReproductiveHealthProfile_ForFemaleUser_Returns204AndDeletesProfile()
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
                ContraceptionMethodCode: null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        HttpResponseMessage response = await Client.DeleteAsync(
            "/api/user-profile/reproductive-health",
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await UserProfilePersistenceAssertions.AssertReproductiveProfileMissingAsync(
            Factory.Services,
            member.UserId,
            CancellationToken.None);
    }
}
