using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.FoodFeature.AddFood;
using DrugsApp.Features.FoodFeature.AddFoodGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.FoodFeature.AddFoodGroup;

public sealed class AddFoodGroupReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddFoodGroup_WithTwoFoods_Returns201AndCreatedGroup()
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

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/food-groups",
            new AddFoodGroupRequest(groupName, [firstFood.Food.Id, secondFood.Food.Id]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedFoodGroupResponse? payload = await response.Content.ReadFromJsonAsync<CreatedFoodGroupResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.FoodGroup.Id.Should().NotBe(Guid.Empty);
        payload.FoodGroup.Name.Should().Be(groupName);
        payload.FoodGroup.Foods.Should().HaveCount(2);
        payload.FoodGroup.Foods.Select(food => food.Id)
            .Should()
            .BeEquivalentTo([firstFood.Food.Id, secondFood.Food.Id]);
    }
}
