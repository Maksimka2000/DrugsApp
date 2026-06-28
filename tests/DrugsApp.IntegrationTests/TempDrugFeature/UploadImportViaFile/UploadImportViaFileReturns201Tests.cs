using System.Net;
using System.Net.Http.Json;
using DrugsApp.Features.TempDrugFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.TempDrugFeature.UploadImportViaFile;

public sealed class UploadImportViaFileReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task UploadImportViaFile_WithValidXlsx_Returns201AndCreatesBatch()
    {
        await TempDrugTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        byte[] workbookBytes = TempDrugTestExcel.CreateSampleWorkbookBytes();

        HttpResponseMessage response = await Client.PostAsync(
            "/api/temp-drug-imports/file-upload",
            CreateMultipartContent(workbookBytes),
            CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreatedImportBatchResponse? payload = await response.Content.ReadFromJsonAsync<CreatedImportBatchResponse>(
            IntegrationTestJson.Options,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.BatchId.Should().NotBe(Guid.Empty);
        payload.TotalQuantityCount.Should().Be(1);
        payload.Headers.Should().Contain(["CommercialName", "ProductIdentifier", "ActiveSubstance"]);
        payload.TempDrugFields.Should().NotBeEmpty();
        payload.DefaultMapping.Should().ContainKey("CommercialName");

        await TempDrugPersistenceAssertions.AssertImportBatchExistsAsync(
            Factory.Services,
            payload.BatchId,
            CancellationToken.None);
    }

    private static MultipartFormDataContent CreateMultipartContent(byte[] workbookBytes)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(workbookBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileContent, "File", "integration-import.xlsx");
        return content;
    }
}
