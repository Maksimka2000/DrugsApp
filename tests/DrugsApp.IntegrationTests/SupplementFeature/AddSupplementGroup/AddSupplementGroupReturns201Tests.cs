using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.SupplementFeature.AddSupplement;
using DrugsApp.Features.SupplementFeature.AddSupplementGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.SupplementFeature.AddSupplementGroup;

public sealed class AddSupplementGroupReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddSupplementGroup_WithTwoSupplements_Returns201AndCreatedGroup()
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

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/supplement-groups",
            new AddSupplementGroupRequest(groupName, [firstSupplement.Supplement.Id, secondSupplement.Supplement.Id]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedSupplementGroupResponse? payload = await response.Content.ReadFromJsonAsync<CreatedSupplementGroupResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.SupplementGroup.Id.Should().NotBe(Guid.Empty);
        payload.SupplementGroup.Name.Should().Be(groupName);
        payload.SupplementGroup.Supplements.Should().HaveCount(2);
        payload.SupplementGroup.Supplements.Select(supplement => supplement.Id)
            .Should()
            .BeEquivalentTo([firstSupplement.Supplement.Id, secondSupplement.Supplement.Id]);
    }
}
