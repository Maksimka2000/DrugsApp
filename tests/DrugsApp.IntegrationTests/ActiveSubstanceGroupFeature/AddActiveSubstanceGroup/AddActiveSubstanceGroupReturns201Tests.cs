using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup;

public sealed class AddActiveSubstanceGroupReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddActiveSubstanceGroup_WithTwoSubstances_Returns201AndCreatedGroup()
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

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/active-substance-groups",
            new AddActiveSubstanceGroupRequest(
                groupName,
                [firstSubstance.Id, secondSubstance.Id]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedActiveSubstanceGroupResponse? payload = await response.Content.ReadFromJsonAsync<CreatedActiveSubstanceGroupResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().NotBe(Guid.Empty);
        payload.Name.Should().Be(groupName);
        payload.Members.Should().HaveCount(2);
        payload.Members.Select(member => member.Id)
            .Should()
            .BeEquivalentTo([firstSubstance.Id, secondSubstance.Id]);
    }
}
