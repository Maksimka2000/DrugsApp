using System.Net.Http.Json;
using DrugsApp.Features.DrugFeature.UpdateDrug;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.DrugFeature.UpdateDrug;

public sealed class UpdateDrugReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateDrug_WithCommercialName_Returns200AndUpdatedDrug()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        const string updatedName = "Nurofen Integration Updated";

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/drugs/{CatalogTestSeed.NurofenDrugId}",
            new UpdateDrugRequest(CommercialName: updatedName, NewActiveSubstanceIds: null, TypeOfDrug: null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedDrugResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedDrugResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(CatalogTestSeed.NurofenDrugId);
        payload.CommercialName.Should().Be(updatedName);
        payload.ActiveSubstances.Should().ContainSingle(substance => substance.Id == CatalogTestSeed.IbuprofenId);
    }
}
