using System.Net.Http.Json;
using DrugsApp.Features.SupplementFeature.AddSupplement;
using DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.SupplementFeature.UpdateSupplementGroup;

public sealed class UpdateSupplementGroupReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateSupplementGroup_WithNewName_Returns200AndUpdatedGroup()
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

        Guid[] memberIds = [firstSupplement.Supplement.Id, secondSupplement.Supplement.Id];

        var createdGroup = await SupplementTestFlows.AddSupplementGroupAsync(
            Client,
            SupplementTestFlows.CreateUniqueSupplementGroupName(),
            memberIds,
            CancellationToken.None);

        string updatedName = SupplementTestFlows.CreateUniqueSupplementGroupName();

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/supplement-groups/{createdGroup.SupplementGroup.Id}",
            new UpdateSupplementGroupRequest(updatedName, memberIds),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedSupplementGroupResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedSupplementGroupResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.SupplementGroup.Id.Should().Be(createdGroup.SupplementGroup.Id);
        payload.SupplementGroup.Name.Should().Be(updatedName);
        payload.SupplementGroup.Supplements.Should().HaveCount(2);
        payload.SupplementGroup.Supplements.Select(supplement => supplement.Id)
            .Should()
            .BeEquivalentTo(memberIds);
    }
}
