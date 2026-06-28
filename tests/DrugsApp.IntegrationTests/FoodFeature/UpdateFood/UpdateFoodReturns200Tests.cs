using System.Net.Http.Json;
using DrugsApp.Features.FoodFeature.UpdateFood;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.FoodFeature.UpdateFood;

public sealed class UpdateFoodReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateFood_WithNewName_Returns200AndUpdatedFood()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        const string updatedName = "Grapefruit integration updated";

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/foods/{CatalogTestSeed.GrapefruitFoodId}",
            new UpdateFoodRequest(updatedName, null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedFoodResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedFoodResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Food.Id.Should().Be(CatalogTestSeed.GrapefruitFoodId);
        payload.Food.Name.Should().Be(updatedName);
    }
}
