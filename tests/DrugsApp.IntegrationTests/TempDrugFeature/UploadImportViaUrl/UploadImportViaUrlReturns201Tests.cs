using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.TempDrugFeature.Shared;
using DrugsApp.Features.TempDrugFeature.UploadImportViaUrl;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.TempDrugFeature.UploadImportViaUrl;

public sealed class UploadImportViaUrlReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task UploadImportViaUrl_WithDownloadableXlsx_Returns201AndCreatesBatch()
    {
        await TempDrugTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/temp-drug-imports/url-upload",
            new UploadImportViaUrlRequest("http://integration.test/import.xlsx"),
            IntegrationTestJson.Options,
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedImportBatchResponse? payload = await response.Content.ReadFromJsonAsync<CreatedImportBatchResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.BatchId.Should().NotBe(Guid.Empty);
        payload.TotalQuantityCount.Should().Be(1);
        payload.Headers.Should().Contain("CommercialName");

        await TempDrugPersistenceAssertions.AssertImportBatchExistsAsync(
            Factory.Services,
            payload.BatchId,
            CancellationToken.None);
    }
}
