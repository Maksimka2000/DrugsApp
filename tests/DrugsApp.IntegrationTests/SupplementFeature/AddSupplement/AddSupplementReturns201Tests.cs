using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.SupplementFeature.AddSupplement;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.SupplementFeature.AddSupplement;

public sealed class AddSupplementReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddSupplement_WithUniqueName_Returns201AndCreatedSupplement()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string name = SupplementTestFlows.CreateUniqueSupplementName();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/supplements",
            new AddSupplementRequest(name, ["alias-one", "alias-two"]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedSupplementResponse? payload = await response.Content.ReadFromJsonAsync<CreatedSupplementResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Supplement.Id.Should().NotBe(Guid.Empty);
        payload.Supplement.Name.Should().Be(name);
        payload.Supplement.Aliases.Should().BeEquivalentTo(["alias-one", "alias-two"]);
    }
}
