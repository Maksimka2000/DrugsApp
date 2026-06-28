using System.Net.Http.Json;
using DrugsApp.Features.PharmaceuticalFormFeature.AddPharmaceuticalForm;

namespace DrugsApp.IntegrationTests.Common;

internal static class PharmaceuticalFormTestFlows
{
    internal static string CreateUniqueFormulationName() => $"Integration-PharmaForm-{Guid.NewGuid():N}";

    internal static async Task<CreatedPharmaceuticalFormResponse> AddPharmaceuticalFormAsync(
        HttpClient client,
        string formulationName,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/pharmaceutical-forms",
            new AddPharmaceuticalFormRequest(formulationName),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreatedPharmaceuticalFormResponse? payload = await response.Content.ReadFromJsonAsync<CreatedPharmaceuticalFormResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Add pharmaceutical form response body was empty.");
    }
}
