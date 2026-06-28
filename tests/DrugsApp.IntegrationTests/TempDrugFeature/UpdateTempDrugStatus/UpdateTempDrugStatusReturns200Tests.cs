using System.Net.Http.Json;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.TempDrugFeature.UpdateTempDrugStatus;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.TempDrugFeature.UpdateTempDrugStatus;

public sealed class UpdateTempDrugStatusReturns200Tests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateTempDrugStatus_WithReviewLater_Returns200AndPersistsStatus()
    {
        await TempDrugTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        Guid tempDrugId = await TempDrugTestFlows.InsertPendingVetTempDrugAsync(
            Factory.Services,
            CancellationToken.None);

        const string note = "Deferred during integration test.";

        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/temp-drugs/{tempDrugId}/status",
            new UpdateTempDrugStatusRequest(TempDrugStatus.ReviewLater, note),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.EnsureSuccessStatusCode();

        UpdatedTempDrugStatusResponse? payload = await response.Content.ReadFromJsonAsync<UpdatedTempDrugStatusResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Id.Should().Be(tempDrugId);
        payload.Status.Should().Be(TempDrugStatus.ReviewLater);
        payload.Note.Should().Be(note);

        await TempDrugPersistenceAssertions.AssertTempDrugStatusAsync(
            Factory.Services,
            tempDrugId,
            TempDrugStatus.ReviewLater,
            CancellationToken.None);
    }
}
