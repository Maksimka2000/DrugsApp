using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.AddOralMedicationIntakeGuidelines;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.ActiveSubstanceFeature.AddOralMedicationIntakeGuidelines;

public sealed class AddOralMedicationIntakeGuidelinesReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddOralMedicationIntakeGuidelines_ForSubstanceWithoutGuidelines_Returns201()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string substanceName = CatalogTestFlows.CreateUniqueSubstanceName();
        var createdSubstance = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            substanceName,
            CancellationToken.None);

        var request = new AddOralMedicationIntakeGuidelinesRequest(
            FoodIntakeType: FoodIntakeType.WithMeal,
            WaitBeforeMinutes: null,
            WaitAfterMinutes: null,
            MealNotes: "Take with breakfast.",
            AvoidFoodDrug: null,
            AvoidFoodTherapy: null,
            LiquidNotes: null,
            LiquidInstructions: null,
            RecommendedMorning: true,
            RecommendedEvening: null);

        // Host JSON options serialize enums as integers, not camelCase strings.
        string requestJson = JsonSerializer.Serialize(request, IntegrationTestJson.ApiRequestOptions);
        using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await Client.PostAsync(
            $"/api/active-substances/{createdSubstance.Id}/oral-medication-intake-guidelines",
            content,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedOralMedicationIntakeGuidelinesResponse? payload =
            await response.Content.ReadFromJsonAsync<CreatedOralMedicationIntakeGuidelinesResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.ActiveSubstanceId.Should().Be(createdSubstance.Id);
        payload.OralMedicationIntakeGuidelines.FoodIntakeType.Should().Be(FoodIntakeType.WithMeal);
        payload.OralMedicationIntakeGuidelines.MealNotes.Should().Be("Take with breakfast.");
        payload.OralMedicationIntakeGuidelines.RecommendedMorning.Should().BeTrue();
    }
}
