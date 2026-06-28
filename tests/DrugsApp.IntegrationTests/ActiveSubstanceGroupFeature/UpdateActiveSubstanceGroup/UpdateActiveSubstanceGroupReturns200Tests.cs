using System.Net.Http.Json;
using DrugsApp.Features.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;

public sealed class UpdateActiveSubstanceGroupReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateActiveSubstanceGroup_WithNewName_Returns200AndUpdatedGroup()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        const string updatedName = "NSAID cluster integration updated";

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/active-substance-groups/{CatalogTestSeed.NsaidGroupId}",
            new UpdateActiveSubstanceGroupRequest(updatedName, null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedActiveSubstanceGroupResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedActiveSubstanceGroupResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(CatalogTestSeed.NsaidGroupId);
        payload.Name.Should().Be(updatedName);
        payload.Members.Should().HaveCount(5);
    }
}
