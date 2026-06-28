using System.Net.Http.Json;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.Features.UserProfileFeature.UpdateUserProfile;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserProfileFeature.UpdateUserProfile;

public sealed class UpdateUserProfileReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateUserProfile_WithNewPersonalData_Returns200AndUpdatedProfile()
    {
        AuthenticatedMember member = await UserProfileTestFlows.RegisterConfirmAndLoginAsync(
            Client,
            EmailSender,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, member.AccessToken);

        const string updatedFirstName = "Integration-Updated";
        const string updatedLastName = "Tester-Updated";
        DateOnly updatedDateOfBirth = new(1994, 8, 20);

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            "/api/user-profile",
            new UpdateUserProfileRequest(
                FirstName: updatedFirstName,
                LastName: updatedLastName,
                DateOfBirth: updatedDateOfBirth,
                Gender: ProfileGender.female.ToString(),
                ConfirmReproductiveHealthDeletion: null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UserProfileResponse? payload = await response.Content.ReadFromJsonAsync<UserProfileResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.FirstName.Should().Be(updatedFirstName);
        payload.LastName.Should().Be(updatedLastName);
        payload.DateOfBirth.Should().Be(updatedDateOfBirth);
        payload.Gender.Should().Be(ProfileGender.female);
        payload.HasReproductiveHealthProfile.Should().BeFalse();
    }
}
