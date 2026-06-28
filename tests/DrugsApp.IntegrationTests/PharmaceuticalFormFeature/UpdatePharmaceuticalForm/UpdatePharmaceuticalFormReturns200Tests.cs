using System.Net.Http.Json;
using DrugsApp.Features.PharmaceuticalFormFeature.UpdatePharmaceuticalForm;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.PharmaceuticalFormFeature.UpdatePharmaceuticalForm;

public sealed class UpdatePharmaceuticalFormReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdatePharmaceuticalForm_WithNewName_Returns200AndPersistsForm()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        var created = await PharmaceuticalFormTestFlows.AddPharmaceuticalFormAsync(
            Client,
            PharmaceuticalFormTestFlows.CreateUniqueFormulationName(),
            CancellationToken.None);

        string updatedName = PharmaceuticalFormTestFlows.CreateUniqueFormulationName();

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/pharmaceutical-forms/{created.Id}",
            new UpdatePharmaceuticalFormRequest(updatedName),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedPharmaceuticalFormResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedPharmaceuticalFormResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(created.Id);
        payload.FormulationName.Should().Be(updatedName);

        await PharmaceuticalFormPersistenceAssertions.AssertFormulationNameAsync(
            Factory.Services,
            created.Id,
            updatedName,
            CancellationToken.None);
    }
}
