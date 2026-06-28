using System.Net.Http.Json;
using DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;
using DrugsApp.Features.AuthFeature.LoginAdmin;

namespace DrugsApp.IntegrationTests.Common;

internal static class CatalogTestFlows
{
    internal static async Task AuthenticateAsAdminAsync(HttpClient client, CancellationToken cancellationToken)
    {
        LoginAdminResponse login = await TestAuthHelper.LoginAsAdminAsync(client, cancellationToken);
        TestAuthHelper.ApplyBearerToken(client, login.AccessToken);
    }

    internal static async Task<CreatedActiveSubstanceResponse> AddActiveSubstanceAsync(
        HttpClient client,
        string name,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/active-substances",
            new AddActiveSubstanceRequest(name, false, null, null),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreatedActiveSubstanceResponse? payload = await response.Content.ReadFromJsonAsync<CreatedActiveSubstanceResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Add active substance response body was empty.");
    }

    internal static string CreateUniqueSubstanceName() => $"Integration-Substance-{Guid.NewGuid():N}";

    internal static string CreateUniqueGroupName() => $"Integration-Group-{Guid.NewGuid():N}";
}
