using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.RouteOfAdministrationFeature.AddRouteOfAdministration;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.RouteOfAdministrationFeature.AddRouteOfAdministration;

public sealed class AddRouteOfAdministrationReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddRouteOfAdministration_WithUniqueName_Returns201AndPersistsRoute()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string routeName = RouteOfAdministrationTestFlows.CreateUniqueRouteName();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/routes-of-administration",
            new AddRouteOfAdministrationRequest(routeName, IsOralIntake: true),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedRouteOfAdministrationResponse? payload = await response.Content.ReadFromJsonAsync<CreatedRouteOfAdministrationResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().NotBe(Guid.Empty);
        payload.RouteOfAdministrationName.Should().Be(routeName);
        payload.IsOralIntake.Should().BeTrue();

        await RouteOfAdministrationPersistenceAssertions.AssertRouteAsync(
            Factory.Services,
            payload.Id,
            routeName,
            expectedIsOralIntake: true,
            CancellationToken.None);
    }
}
