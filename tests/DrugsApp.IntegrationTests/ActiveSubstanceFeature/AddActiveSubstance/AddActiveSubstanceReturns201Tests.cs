using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.ActiveSubstanceFeature.AddActiveSubstance;

public sealed class AddActiveSubstanceReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddActiveSubstance_WithUniqueName_Returns201AndCreatedSubstance()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string name = CatalogTestFlows.CreateUniqueSubstanceName();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/active-substances",
            new AddActiveSubstanceRequest(name, false, null, null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedActiveSubstanceResponse? payload = await response.Content.ReadFromJsonAsync<CreatedActiveSubstanceResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().NotBe(Guid.Empty);
        payload.ActiveSubstanceName.Should().Be(name);
        payload.HasAlcoholInteraction.Should().BeFalse();
        payload.OralMedicationIntakeGuidelines.Should().BeNull();
    }
}
