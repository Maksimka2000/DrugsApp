using System.Net.Http.Json;
using DrugsApp.Features.DrugFeature.UpdateDrugForm;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.DrugFeature.UpdateDrugForm;

public sealed class UpdateDrugFormReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateDrugForm_WithDosage_Returns200AndUpdatedForm()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        const string updatedDosage = "400 mg";

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/drugs/{CatalogTestSeed.NurofenDrugId}/drug-forms/{CatalogTestSeed.NurofenDrugFormId}",
            new UpdateDrugFormRequest(
                AtcCode: null,
                ClearAtcCode: null,
                Dosage: updatedDosage,
                ClearDosage: null,
                Leaflet: null,
                ClearLeaflet: null,
                EducationMaterials: null,
                ClearEducationMaterials: null,
                RouteOfAdministrationIds: null,
                PharmaceuticalFormIds: null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedDrugFormResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedDrugFormResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.DrugId.Should().Be(CatalogTestSeed.NurofenDrugId);
        payload.DrugFormId.Should().Be(CatalogTestSeed.NurofenDrugFormId);
        payload.Dosage.Should().Be(updatedDosage);
    }
}
