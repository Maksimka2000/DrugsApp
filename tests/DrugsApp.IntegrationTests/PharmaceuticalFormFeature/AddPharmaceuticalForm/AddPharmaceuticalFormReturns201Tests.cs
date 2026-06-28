using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.PharmaceuticalFormFeature.AddPharmaceuticalForm;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.PharmaceuticalFormFeature.AddPharmaceuticalForm;

public sealed class AddPharmaceuticalFormReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task AddPharmaceuticalForm_WithUniqueName_Returns201AndPersistsForm()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        string formulationName = PharmaceuticalFormTestFlows.CreateUniqueFormulationName();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/pharmaceutical-forms",
            new AddPharmaceuticalFormRequest(formulationName),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedPharmaceuticalFormResponse? payload = await response.Content.ReadFromJsonAsync<CreatedPharmaceuticalFormResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().NotBe(Guid.Empty);
        payload.FormulationName.Should().Be(formulationName);

        await PharmaceuticalFormPersistenceAssertions.AssertFormulationNameAsync(
            Factory.Services,
            payload.Id,
            formulationName,
            CancellationToken.None);
    }
}
