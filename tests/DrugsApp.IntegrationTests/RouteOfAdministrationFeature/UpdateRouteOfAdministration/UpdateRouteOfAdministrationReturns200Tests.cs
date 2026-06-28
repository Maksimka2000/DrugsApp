using System.Net.Http.Json;
using DrugsApp.Features.RouteOfAdministrationFeature.UpdateRouteOfAdministration;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.RouteOfAdministrationFeature.UpdateRouteOfAdministration;

public sealed class UpdateRouteOfAdministrationReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateRouteOfAdministration_WithNewNameAndOralFlag_Returns200AndPersistsRoute()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        var created = await RouteOfAdministrationTestFlows.AddRouteOfAdministrationAsync(
            Client,
            RouteOfAdministrationTestFlows.CreateUniqueRouteName(),
            CancellationToken.None,
            isOralIntake: false);

        string updatedName = RouteOfAdministrationTestFlows.CreateUniqueRouteName();

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/routes-of-administration/{created.Id}",
            new UpdateRouteOfAdministrationRequest(updatedName, IsOralIntake: true),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedRouteOfAdministrationResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedRouteOfAdministrationResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(created.Id);
        payload.RouteOfAdministrationName.Should().Be(updatedName);
        payload.IsOralIntake.Should().BeTrue();

        await RouteOfAdministrationPersistenceAssertions.AssertRouteAsync(
            Factory.Services,
            created.Id,
            updatedName,
            expectedIsOralIntake: true,
            CancellationToken.None);
    }
}
