using System.Net.Http.Json;
using DrugsApp.Features.SupplementFeature.AddSupplement;
using DrugsApp.Features.SupplementFeature.AddSupplementGroup;

namespace DrugsApp.IntegrationTests.Common;

internal static class SupplementTestFlows
{
    internal static async Task<CreatedSupplementResponse> AddSupplementAsync(
        HttpClient client,
        string name,
        CancellationToken cancellationToken,
        IReadOnlyCollection<string>? aliases = null)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/supplements",
            new AddSupplementRequest(name, aliases),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreatedSupplementResponse? payload = await response.Content.ReadFromJsonAsync<CreatedSupplementResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Add supplement response body was empty.");
    }

    internal static async Task<CreatedSupplementGroupResponse> AddSupplementGroupAsync(
        HttpClient client,
        string name,
        IReadOnlyCollection<Guid> supplementIds,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/supplement-groups",
            new AddSupplementGroupRequest(name, supplementIds),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreatedSupplementGroupResponse? payload = await response.Content.ReadFromJsonAsync<CreatedSupplementGroupResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Add supplement group response body was empty.");
    }

    internal static string CreateUniqueSupplementName() => $"Integration-Supplement-{Guid.NewGuid():N}";

    internal static string CreateUniqueSupplementGroupName() => $"Integration-SupplementGroup-{Guid.NewGuid():N}";
}
