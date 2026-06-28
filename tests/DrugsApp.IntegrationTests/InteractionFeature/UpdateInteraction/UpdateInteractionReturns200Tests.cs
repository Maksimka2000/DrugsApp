using System.Net.Http.Json;
using DrugsApp.Features.InteractionFeature.UpdateInteraction;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.InteractionFeature.UpdateInteraction;

public sealed class UpdateInteractionReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateInteraction_WithNewTextAndSeverity_Returns200AndKeepsPartiesAndExpandedRows()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        var substance = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        var food = await FoodTestFlows.AddFoodAsync(
            Client,
            FoodTestFlows.CreateUniqueFoodName(),
            CancellationToken.None);

        var created = await InteractionTestFlows.CreateSubstanceFoodInteractionAsync(
            Client,
            substance.Id,
            food.Food.Id,
            CancellationToken.None);

        const string updatedDefaultText = "Updated integration interaction guidance.";
        const string updatedStomachText = "Updated stomach-specific note.";

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/active-substance-interactions/{created.InteractionId}",
            new UpdateInteractionRequest(
                DefaultText: updatedDefaultText,
                StomachText: updatedStomachText,
                SeverityId: InteractionTestSeed.ModerateSeverityId,
                ClearDefaultText: null,
                ClearStomachText: null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedInteractionResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedInteractionResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.InteractionId.Should().Be(created.InteractionId);
        payload.Severity.Id.Should().Be(InteractionTestSeed.ModerateSeverityId);
        payload.DefaultText.Should().Be(updatedDefaultText);
        payload.StomachText.Should().Be(updatedStomachText);

        await InteractionPersistenceAssertions.AssertSubstanceFoodPartiesAndExpandedAsync(
            Factory.Services,
            created.InteractionId,
            substance.Id,
            food.Food.Id,
            CancellationToken.None);
    }
}
