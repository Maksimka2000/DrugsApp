using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.FoodFeature.AddFood;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.FoodFeature.AddFood;

public sealed class AddFoodReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddFood_WithUniqueName_Returns201AndCreatedFood()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string name = FoodTestFlows.CreateUniqueFoodName();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/foods",
            new AddFoodRequest(name, ["alias-one", "alias-two"]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedFoodResponse? payload = await response.Content.ReadFromJsonAsync<CreatedFoodResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Food.Id.Should().NotBe(Guid.Empty);
        payload.Food.Name.Should().Be(name);
        payload.Food.Aliases.Should().BeEquivalentTo(["alias-one", "alias-two"]);
    }
}
