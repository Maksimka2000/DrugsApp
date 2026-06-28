using System.Net.Http.Json;
using DrugsApp.Features.FoodFeature.RemoveFood;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.FoodFeature.RemoveFood;

public sealed class RemoveFoodReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task RemoveFood_WhenNotInFoodGroup_Returns200AndDeletedFood()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string foodName = FoodTestFlows.CreateUniqueFoodName();
        var createdFood = await FoodTestFlows.AddFoodAsync(Client, foodName, CancellationToken.None);

        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/foods/{createdFood.Food.Id}",
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        DeletedFoodResponse? payload = await response.Content.ReadFromJsonAsync<DeletedFoodResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Food.Id.Should().Be(createdFood.Food.Id);
        payload.Food.Name.Should().Be(foodName);
    }
}
