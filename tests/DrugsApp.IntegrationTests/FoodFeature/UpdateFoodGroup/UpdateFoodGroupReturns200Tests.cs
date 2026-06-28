using System.Net.Http.Json;
using DrugsApp.Features.FoodFeature.UpdateFoodGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.FoodFeature.UpdateFoodGroup;

public sealed class UpdateFoodGroupReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateFoodGroup_WithNewName_Returns200AndUpdatedGroup()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        const string updatedName = "Citrus & fruit integration updated";

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/food-groups/{CatalogTestSeed.CitrusFoodGroupId}",
            new UpdateFoodGroupRequest(updatedName, CatalogTestSeed.CitrusFoodGroupMemberIds),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedFoodGroupResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedFoodGroupResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.FoodGroup.Id.Should().Be(CatalogTestSeed.CitrusFoodGroupId);
        payload.FoodGroup.Name.Should().Be(updatedName);
        payload.FoodGroup.Foods.Should().HaveCount(5);
    }
}
