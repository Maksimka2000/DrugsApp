using System.Net.Http.Json;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Features.InteractionFeature.CreateInteraction;

namespace DrugsApp.IntegrationTests.Common;

internal static class InteractionTestFlows
{
    internal static async Task<CreatedInteractionResponse> CreateSubstanceFoodInteractionAsync(
        HttpClient client,
        Guid substanceId,
        Guid foodId,
        CancellationToken cancellationToken,
        string defaultText = "Integration interaction default text.",
        string? stomachText = "Take with caution around meals.")
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/active-substance-interactions",
            new CreateInteractionRequest(
                Left: new InteractionSideRequest(InteractionType.Substance, substanceId),
                Right: new InteractionSideRequest(InteractionType.Food, foodId),
                SeverityId: InteractionTestSeed.MildSeverityId,
                DefaultText: defaultText,
                StomachText: stomachText),
            IntegrationTestJson.Options,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        CreatedInteractionResponse? payload = await response.Content.ReadFromJsonAsync<CreatedInteractionResponse>(
            IntegrationTestJson.Options,
            cancellationToken);

        return payload ?? throw new InvalidOperationException("Create interaction response body was empty.");
    }
}
