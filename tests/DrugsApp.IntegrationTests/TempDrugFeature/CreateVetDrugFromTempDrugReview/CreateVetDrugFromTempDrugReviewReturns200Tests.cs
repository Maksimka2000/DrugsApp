using System.Net.Http.Json;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.TempDrugFeature.CreateVetDrugFromTempDrugReview;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.TempDrugFeature.CreateVetDrugFromTempDrugReview;

public sealed class CreateVetDrugFromTempDrugReviewReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task CreateVetDrugFromTempDrugReview_WithPendingVetTempDrug_Returns200AndLinksVetDrug()
    {
        await TempDrugTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        Guid tempDrugId = await TempDrugTestFlows.InsertPendingVetTempDrugAsync(
            Factory.Services,
            CancellationToken.None);

        string productIdentifier = TempDrugTestFlows.CreateUniqueNumericProductIdentifier();
        string commercialName = $"Integration Vet Candidate {Guid.NewGuid():N}";

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/temp-drugs/{tempDrugId}/vet-drugs",
            new CreateVetDrugFromTempDrugReviewRequest(
                Note: "Created from integration test.",
                CommercialName: commercialName,
                TypeOfProduct: "Weterynaryjny",
                ProductIdentifier: productIdentifier),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        CreatedVetDrugFromTempDrugReviewResponse? payload =
            await response.Content.ReadFromJsonAsync<CreatedVetDrugFromTempDrugReviewResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.TempDrugId.Should().Be(tempDrugId);
        payload.NewStatus.Should().Be(TempDrugStatus.ReviewedAdded);
        payload.LinkedVetDrugId.Should().NotBe(Guid.Empty);
        payload.VetDrug.CommercialName.Should().Be(commercialName);
        payload.VetDrug.ProductIdentifier.Should().Be(int.Parse(productIdentifier));

        await TempDrugPersistenceAssertions.AssertTempDrugStatusAsync(
            Factory.Services,
            tempDrugId,
            TempDrugStatus.ReviewedAdded,
            CancellationToken.None);
    }
}
