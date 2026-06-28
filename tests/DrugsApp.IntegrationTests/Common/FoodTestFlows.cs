using System.Net.Http.Json;
using DrugsApp.Features.FoodFeature.AddFood;
using DrugsApp.Features.FoodFeature.AddFoodGroup;
using DrugsApp.Features.FoodFeature.UpdateFood;

namespace DrugsApp.IntegrationTests.Common;

internal static class FoodTestFlows
{
    internal static async Task<CreatedFoodResponse> AddFoodAsync(
        HttpClient client,
        string name,
        CancellationToken cancellationToken,
        IReadOnlyCollection<string>? aliases = null)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/foods",
            new AddFoodRequest(name, aliases),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreatedFoodResponse? payload = await response.Content.ReadFromJsonAsync<CreatedFoodResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Add food response body was empty.");
    }

    internal static async Task<CreatedFoodGroupResponse> AddFoodGroupAsync(
        HttpClient client,
        string name,
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/food-groups",
            new AddFoodGroupRequest(name, memberIds),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreatedFoodGroupResponse? payload = await response.Content.ReadFromJsonAsync<CreatedFoodGroupResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Add food group response body was empty.");
    }

    internal static string CreateUniqueFoodName() => $"Integration-Food-{Guid.NewGuid():N}";

    internal static string CreateUniqueFoodGroupName() => $"Integration-FoodGroup-{Guid.NewGuid():N}";
}
