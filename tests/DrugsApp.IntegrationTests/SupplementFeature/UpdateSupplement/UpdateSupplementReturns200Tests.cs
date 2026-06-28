using System.Net.Http.Json;
using DrugsApp.Features.SupplementFeature.UpdateSupplement;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.SupplementFeature.UpdateSupplement;

public sealed class UpdateSupplementReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateSupplement_WithNewNameAndAliases_Returns200AndUpdatedSupplement()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        var created = await SupplementTestFlows.AddSupplementAsync(
            Client,
            SupplementTestFlows.CreateUniqueSupplementName(),
            CancellationToken.None,
            aliases: ["original-alias"]);

        string updatedName = SupplementTestFlows.CreateUniqueSupplementName();
        string[] updatedAliases = ["updated-alias-one", "updated-alias-two"];

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/supplements/{created.Supplement.Id}",
            new UpdateSupplementRequest(updatedName, updatedAliases),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedSupplementResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedSupplementResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Supplement.Id.Should().Be(created.Supplement.Id);
        payload.Supplement.Name.Should().Be(updatedName);
        payload.Supplement.Aliases.Should().BeEquivalentTo(updatedAliases);
    }
}
