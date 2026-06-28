using System.Net.Http.Json;
using DrugsApp.Features.SupplementFeature.DeleteSupplement;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.SupplementFeature.DeleteSupplement;

public sealed class DeleteSupplementReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task DeleteSupplement_WhenNotInSupplementGroup_Returns200AndDeletedSupplement()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string supplementName = SupplementTestFlows.CreateUniqueSupplementName();
        var created = await SupplementTestFlows.AddSupplementAsync(Client, supplementName, CancellationToken.None);

        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/supplements/{created.Supplement.Id}",
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        DeleteSupplementResponse? payload = await response.Content.ReadFromJsonAsync<DeleteSupplementResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Supplement.Id.Should().Be(created.Supplement.Id);
        payload.Supplement.Name.Should().Be(supplementName);
    }
}
