using System.Net.Http.Json;
using DrugsApp.Features.ActiveSubstanceFeature.DeleteActiveSubstance;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.ActiveSubstanceFeature.DeleteActiveSubstance;

public sealed class DeleteActiveSubstanceReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task DeleteActiveSubstance_WhenNotReferencedByDrug_Returns200AndDeletedSubstance()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string substanceName = CatalogTestFlows.CreateUniqueSubstanceName();
        var createdSubstance = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            substanceName,
            CancellationToken.None);

        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/active-substances/{createdSubstance.Id}",
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        DeleteActiveSubstanceResponse? payload = await response.Content.ReadFromJsonAsync<DeleteActiveSubstanceResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.ActiveSubstance.Id.Should().Be(createdSubstance.Id);
        payload.ActiveSubstance.ActiveSubstanceName.Should().Be(substanceName);
    }
}
