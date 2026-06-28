using System.Net.Http.Json;
using DrugsApp.Features.FoodFeature.AddFood;
using DrugsApp.Features.FoodFeature.AddFoodGroup;
using DrugsApp.Features.FoodFeature.RemoveFoodGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.FoodFeature.RemoveFoodGroup;

public sealed class RemoveFoodGroupReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task RemoveFoodGroup_WithCreatedGroup_Returns200AndDeletedGroup()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        CreatedFoodResponse firstFood = await FoodTestFlows.AddFoodAsync(
            Client,
            FoodTestFlows.CreateUniqueFoodName(),
            CancellationToken.None);

        CreatedFoodResponse secondFood = await FoodTestFlows.AddFoodAsync(
            Client,
            FoodTestFlows.CreateUniqueFoodName(),
            CancellationToken.None);

        string groupName = FoodTestFlows.CreateUniqueFoodGroupName();

        CreatedFoodGroupResponse createdGroup = await FoodTestFlows.AddFoodGroupAsync(
            Client,
            groupName,
            [firstFood.Food.Id, secondFood.Food.Id],
            CancellationToken.None);

        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/food-groups/{createdGroup.FoodGroup.Id}",
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        DeletedFoodGroupResponse? payload = await response.Content.ReadFromJsonAsync<DeletedFoodGroupResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.FoodGroup.Id.Should().Be(createdGroup.FoodGroup.Id);
        payload.FoodGroup.Name.Should().Be(groupName);
    }
}
