using System.Net.Http.Json;
using DrugsApp.Features.ActiveSubstanceFeature.UpdateActiveSubstance;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.ActiveSubstanceFeature.UpdateActiveSubstance;

public sealed class UpdateActiveSubstanceReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateActiveSubstance_WithAlcoholFlag_Returns200AndUpdatedSubstance()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/active-substances/{CatalogTestSeed.IbuprofenId}",
            new UpdateActiveSubstanceRequest(
                ActiveSubstanceName: null,
                HasAlcoholInteraction: true,
                AlcoholSpecialText: "Avoid alcohol during treatment.",
                ClearAlcoholSpecialText: null,
                FoodIntakeType: null,
                WaitBeforeMinutes: null,
                ClearWaitBeforeMinutes: null,
                WaitAfterMinutes: null,
                ClearWaitAfterMinutes: null,
                MealNotes: null,
                ClearMealNotes: null,
                AvoidFoodDrug: null,
                ClearAvoidFoodDrug: null,
                AvoidFoodTherapy: null,
                ClearAvoidFoodTherapy: null,
                LiquidNotes: null,
                ClearLiquidNotes: null,
                LiquidInstructions: null,
                ClearLiquidInstructions: null,
                RecommendedMorning: null,
                ClearRecommendedMorning: null,
                RecommendedEvening: null,
                ClearRecommendedEvening: null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedActiveSubstanceResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedActiveSubstanceResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(CatalogTestSeed.IbuprofenId);
        payload.HasAlcoholInteraction.Should().BeTrue();
        payload.AlcoholSpecialText.Should().Be("Avoid alcohol during treatment.");
    }
}
