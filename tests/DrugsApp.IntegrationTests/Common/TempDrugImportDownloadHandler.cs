using System.Net;
using System.Net.Http.Headers;

namespace DrugsApp.IntegrationTests.Common;

internal sealed class TempDrugImportDownloadHandler(byte[] xlsxBytes) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.MethodNotAllowed));
        }

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(xlsxBytes)
        };

        response.Content.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = "integration-import.xlsx"
        };

        return Task.FromResult(response);
    }
}
