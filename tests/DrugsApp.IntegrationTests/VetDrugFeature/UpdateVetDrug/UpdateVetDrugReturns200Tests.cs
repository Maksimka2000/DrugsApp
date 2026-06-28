using System.Net.Http.Json;
using DrugsApp.Features.VetDrugFeature.UpdateVetDrug;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.VetDrugFeature.UpdateVetDrug;

public sealed class UpdateVetDrugReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateVetDrug_WithCommercialName_Returns200AndUpdatedVetDrug()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        const string updatedName = "Bravecto Integration Updated";

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/vet-drugs/{VetDrugTestSeed.BravectoId}",
            new UpdateVetDrugRequest(CommercialName: updatedName),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedVetDrugResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedVetDrugResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(VetDrugTestSeed.BravectoId);
        payload.CommercialName.Should().Be(updatedName);
    }
}
