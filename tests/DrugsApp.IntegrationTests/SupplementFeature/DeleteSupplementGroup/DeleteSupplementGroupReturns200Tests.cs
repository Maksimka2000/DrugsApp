using System.Net.Http.Json;
using DrugsApp.Features.SupplementFeature.AddSupplement;
using DrugsApp.Features.SupplementFeature.DeleteSupplementGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.SupplementFeature.DeleteSupplementGroup;

public sealed class DeleteSupplementGroupReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task DeleteSupplementGroup_WithCreatedGroup_Returns200AndDeletedGroup()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        CreatedSupplementResponse firstSupplement = await SupplementTestFlows.AddSupplementAsync(
            Client,
            SupplementTestFlows.CreateUniqueSupplementName(),
            CancellationToken.None);

        CreatedSupplementResponse secondSupplement = await SupplementTestFlows.AddSupplementAsync(
            Client,
            SupplementTestFlows.CreateUniqueSupplementName(),
            CancellationToken.None);

        string groupName = SupplementTestFlows.CreateUniqueSupplementGroupName();

        var createdGroup = await SupplementTestFlows.AddSupplementGroupAsync(
            Client,
            groupName,
            [firstSupplement.Supplement.Id, secondSupplement.Supplement.Id],
            CancellationToken.None);

        HttpResponseMessage response = await Client.DeleteAsync(
            $"/api/supplement-groups/{createdGroup.SupplementGroup.Id}",
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        DeleteSupplementGroupResponse? payload = await response.Content.ReadFromJsonAsync<DeleteSupplementGroupResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.SupplementGroup.Id.Should().Be(createdGroup.SupplementGroup.Id);
        payload.SupplementGroup.Name.Should().Be(groupName);
    }
}
