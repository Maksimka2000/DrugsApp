using System.Net.Http.Json;
using DrugsApp.Features.RouteOfAdministrationFeature.AddRouteOfAdministration;

namespace DrugsApp.IntegrationTests.Common;

internal static class RouteOfAdministrationTestFlows
{
    internal static string CreateUniqueRouteName() => $"Integration-Route-{Guid.NewGuid():N}";

    internal static async Task<CreatedRouteOfAdministrationResponse> AddRouteOfAdministrationAsync(
        HttpClient client,
        string routeOfAdministrationName,
        CancellationToken cancellationToken,
        bool? isOralIntake = null)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/routes-of-administration",
            new AddRouteOfAdministrationRequest(routeOfAdministrationName, isOralIntake),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreatedRouteOfAdministrationResponse? payload = await response.Content.ReadFromJsonAsync<CreatedRouteOfAdministrationResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Add route of administration response body was empty.");
    }
}
