using System.Net.Http.Json;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;
using DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.TempDrugFeature.CreateDrugFromTempDrugReview;

public sealed class CreateDrugFromTempDrugReviewReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task CreateDrugFromTempDrugReview_WithPendingHumanTempDrug_Returns200AndCreatesDrug()
    {
        await TempDrugTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string substanceName = CatalogTestFlows.CreateUniqueSubstanceName();
        CreatedActiveSubstanceResponse substance = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            substanceName,
            CancellationToken.None);

        Guid tempDrugId = await TempDrugTestFlows.InsertPendingHumanTempDrugAsync(
            Factory.Services,
            CancellationToken.None,
            activeSubstanceName: substanceName);

        string productIdentifier = TempDrugTestFlows.CreateUniqueNumericProductIdentifier();
        string commercialName = $"Integration Human Drug {Guid.NewGuid():N}";

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/temp-drugs/{tempDrugId}/drugs",
            new CreateDrugFromTempDrugReviewRequest(
                Note: "Created from integration test.",
                CommercialName: commercialName,
                TypeOfProduct: "Human medicinal product",
                TypeOfDrug: DrugType.Standard,
                ActiveSubstanceIds: [substance.Id],
                ProductIdentifier: productIdentifier,
                AtcCode: "N02BE51",
                Dosage: "Integration 500 mg",
                Leaflet: null,
                EducationalMaterials: null,
                RouteOfAdministrationIds: [TempDrugTestSeed.OralRouteId],
                PharmaceuticalFormIds: [TempDrugTestSeed.TabletFormId]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        CreatedDrugFromTempDrugReviewResponse? payload =
            await response.Content.ReadFromJsonAsync<CreatedDrugFromTempDrugReviewResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.TempDrugId.Should().Be(tempDrugId);
        payload.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
        payload.LinkedDrugFormId.Should().NotBe(Guid.Empty);
        payload.Drug.CommercialName.Should().Be(commercialName);
        payload.Drug.DrugForms.Should().ContainSingle(form => form.Id == payload.LinkedDrugFormId);

        await TempDrugPersistenceAssertions.AssertTempDrugStatusAsync(
            Factory.Services,
            tempDrugId,
            TempDrugStatus.ReviewedAdded,
            CancellationToken.None);
    }
}
