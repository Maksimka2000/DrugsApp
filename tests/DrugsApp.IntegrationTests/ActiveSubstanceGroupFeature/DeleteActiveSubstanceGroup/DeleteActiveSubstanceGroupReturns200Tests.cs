using System.Net.Http.Json;
using DrugsApp.Features.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup;
using DrugsApp.Features.ActiveSubstanceGroupFeature.DeleteActiveSubstanceGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.ActiveSubstanceGroupFeature.DeleteActiveSubstanceGroup;

public sealed class DeleteActiveSubstanceGroupReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task DeleteActiveSubstanceGroup_WithExistingGroup_Returns200AndDeletedId()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        var firstSubstance = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        var secondSubstance = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        string groupName = CatalogTestFlows.CreateUniqueGroupName();

        HttpResponseMessage createResponse = await Client.PostAsJsonAsync(
            "/api/active-substance-groups",
            new AddActiveSubstanceGroupRequest(
                groupName,
                [firstSubstance.Id, secondSubstance.Id]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        createResponse.EnsureSuccessStatusCode();

        CreatedActiveSubstanceGroupResponse? createdGroup = await createResponse.Content
            .ReadFromJsonAsync<CreatedActiveSubstanceGroupResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        createdGroup.Should().NotBeNull();

        HttpResponseMessage deleteResponse = await Client.DeleteAsync(
            $"/api/active-substance-groups/{createdGroup!.Id}",
            CancellationToken.None);

        deleteResponse.EnsureSuccessStatusCode();

        DeleteActiveSubstanceGroupResponse? payload = await deleteResponse.Content
            .ReadFromJsonAsync<DeleteActiveSubstanceGroupResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(createdGroup.Id);
    }
}
