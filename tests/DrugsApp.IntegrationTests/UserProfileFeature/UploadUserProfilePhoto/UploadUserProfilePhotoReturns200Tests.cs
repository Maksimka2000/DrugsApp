using System.Net.Http.Json;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserProfileFeature.UploadUserProfilePhoto;

public sealed class UploadUserProfilePhotoReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UploadUserProfilePhoto_WithValidImage_Returns200AndStoresPhoto()
    {
        AuthenticatedMember member = await UserProfileTestFlows.RegisterConfirmAndLoginAsync(
            Client,
            EmailSender,
            CancellationToken.None);

        TestAuthHelper.ApplyBearerToken(Client, member.AccessToken);

        HttpResponseMessage response = await UserProfileTestFlows.UploadProfilePhotoAsync(
            Client,
            UserProfileTestFlows.CreateMinimalPngBytes(),
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UserProfilePhotoResponse? payload = await response.Content.ReadFromJsonAsync<UserProfilePhotoResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.ProfilePhotoUrl.Should().Be("/api/user-profile/photo");

        await UserProfilePersistenceAssertions.AssertProfilePhotoStoredAsync(
            Factory.Services,
            member.UserId,
            CancellationToken.None);
    }
}
