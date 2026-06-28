using System.Net.Http.Json;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;
using DrugsApp.Features.TempDrugFeature.CreateDrugFormFromTempDrugReview;
using DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.TempDrugFeature.CreateDrugFormFromTempDrugReview;

public sealed class CreateDrugFormFromTempDrugReviewReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task CreateDrugFormFromTempDrugReview_WithPendingHumanTempDrug_Returns200AndCreatesDrugForm()
    {
        await TempDrugTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string substanceName = CatalogTestFlows.CreateUniqueSubstanceName();
        CreatedActiveSubstanceResponse substance = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            substanceName,
            CancellationToken.None);

        Guid firstTempDrugId = await TempDrugTestFlows.InsertPendingHumanTempDrugAsync(
            Factory.Services,
            CancellationToken.None,
            activeSubstanceName: substanceName);

        string firstProductIdentifier = TempDrugTestFlows.CreateUniqueNumericProductIdentifier();
        string commercialName = $"Integration Drug Form Parent {Guid.NewGuid():N}";

        HttpResponseMessage createDrugResponse = await Client.PostAsJsonAsync(
            $"/api/temp-drugs/{firstTempDrugId}/drugs",
            new CreateDrugFromTempDrugReviewRequest(
                Note: null,
                CommercialName: commercialName,
                TypeOfProduct: "Human medicinal product",
                TypeOfDrug: DrugType.Standard,
                ActiveSubstanceIds: [substance.Id],
                ProductIdentifier: firstProductIdentifier,
                AtcCode: "N02BE53",
                Dosage: "Integration 100 mg",
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: [TempDrugTestSeed.OralRouteId],
                PharmaceuticalFormIds: [TempDrugTestSeed.TabletFormId]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        createDrugResponse.EnsureSuccessStatusCode();

        CreatedDrugFromTempDrugReviewResponse? createdDrug =
            await createDrugResponse.Content.ReadFromJsonAsync<CreatedDrugFromTempDrugReviewResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        createdDrug.Should().NotBeNull();

        Guid secondTempDrugId = await TempDrugTestFlows.InsertPendingHumanTempDrugAsync(
            Factory.Services,
            CancellationToken.None,
            activeSubstanceName: substanceName);

        string secondProductIdentifier = TempDrugTestFlows.CreateUniqueNumericProductIdentifier();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/temp-drugs/{secondTempDrugId}/drug-forms",
            new CreateDrugFormFromTempDrugReviewRequest(
                Note: "Merged into existing drug.",
                DrugId: createdDrug!.Drug.Id,
                CommercialName: commercialName,
                TypeOfProduct: "Human medicinal product",
                ActiveSubstanceIds: [substance.Id],
                ProductIdentifier: secondProductIdentifier,
                AtcCode: "N02BE54",
                Dosage: "Integration 200 mg",
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: [TempDrugTestSeed.OralRouteId],
                PharmaceuticalFormIds: [TempDrugTestSeed.TabletFormId]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        CreatedDrugFormFromTempDrugReviewResponse? payload =
            await response.Content.ReadFromJsonAsync<CreatedDrugFormFromTempDrugReviewResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.TempDrugId.Should().Be(secondTempDrugId);
        payload.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
        payload.LinkedDrugFormId.Should().NotBe(createdDrug.LinkedDrugFormId);
        payload.DrugForm.Id.Should().Be(payload.LinkedDrugFormId);

        await TempDrugPersistenceAssertions.AssertTempDrugStatusAsync(
            Factory.Services,
            secondTempDrugId,
            TempDrugStatus.ReviewedAdded,
            CancellationToken.None);
    }
}
