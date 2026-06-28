using System.Net.Http.Json;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;
using DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview;
using DrugsApp.Features.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview;

public sealed class UpdateDrugFormLinksFromTempDrugReviewReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateDrugFormLinksFromTempDrugReview_WithPendingHumanTempDrug_Returns200AndUpdatesLinks()
    {
        await TempDrugTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string substanceName = CatalogTestFlows.CreateUniqueSubstanceName();
        CreatedActiveSubstanceResponse substance = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            substanceName,
            CancellationToken.None);

        Guid createTempDrugId = await TempDrugTestFlows.InsertPendingHumanTempDrugAsync(
            Factory.Services,
            CancellationToken.None,
            activeSubstanceName: substanceName);

        string productIdentifier = TempDrugTestFlows.CreateUniqueNumericProductIdentifier();
        string commercialName = $"Integration Link Update Drug {Guid.NewGuid():N}";

        HttpResponseMessage createResponse = await Client.PostAsJsonAsync(
            $"/api/temp-drugs/{createTempDrugId}/drugs",
            new CreateDrugFromTempDrugReviewRequest(
                Note: null,
                CommercialName: commercialName,
                TypeOfProduct: "Human medicinal product",
                TypeOfDrug: DrugType.Standard,
                ActiveSubstanceIds: [substance.Id],
                ProductIdentifier: productIdentifier,
                AtcCode: "N02BE52",
                Dosage: "Integration 250 mg",
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: [TempDrugTestSeed.OralRouteId],
                PharmaceuticalFormIds: [TempDrugTestSeed.TabletFormId]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        createResponse.EnsureSuccessStatusCode();

        CreatedDrugFromTempDrugReviewResponse? createdDrug =
            await createResponse.Content.ReadFromJsonAsync<CreatedDrugFromTempDrugReviewResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        createdDrug.Should().NotBeNull();

        Guid updateTempDrugId = await TempDrugTestFlows.InsertPendingHumanTempDrugAsync(
            Factory.Services,
            CancellationToken.None,
            activeSubstanceName: substanceName);

        const string updatedLeaflet = "https://integration.test/updated-leaflet";

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/temp-drugs/{updateTempDrugId}/drug-form-link-updates",
            new UpdateDrugFormLinksFromTempDrugReviewRequest(
                Note: "Updated links from integration test.",
                DrugId: createdDrug!.Drug.Id,
                DrugFormId: createdDrug.LinkedDrugFormId,
                CommercialName: commercialName,
                TypeOfProduct: "Human medicinal product",
                ActiveSubstanceIds: [substance.Id],
                ProductIdentifier: productIdentifier,
                AtcCode: "N02BE52",
                Dosage: "Integration 250 mg",
                Leaflet: updatedLeaflet,
                EducationalMaterials: null,
                RouteOfAdministrationIds: [TempDrugTestSeed.OralRouteId],
                PharmaceuticalFormIds: [TempDrugTestSeed.TabletFormId]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedDrugFormLinksFromTempDrugReviewResponse? payload =
            await response.Content.ReadFromJsonAsync<UpdatedDrugFormLinksFromTempDrugReviewResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.TempDrugId.Should().Be(updateTempDrugId);
        payload.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
        payload.LinkedDrugFormId.Should().Be(createdDrug.LinkedDrugFormId);
        payload.DrugForm.Id.Should().Be(createdDrug.LinkedDrugFormId);
        payload.DrugForm.Leaflet.Should().Be(updatedLeaflet);

        await TempDrugPersistenceAssertions.AssertTempDrugStatusAsync(
            Factory.Services,
            updateTempDrugId,
            TempDrugStatus.ReviewedAdded,
            CancellationToken.None);
    }
}
