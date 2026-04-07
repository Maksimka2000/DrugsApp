using ClosedXML.Excel;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.TempDrugFeature.ApplicationServices;

internal sealed class TempDrugUploadApplicationService(
    IBlobService _blobService,
    ITempDrugImportBatchRepository _batchRepository,
    IDefaultMappingForColumnsAttributeRepository _defaultMappingRepository,
    IHttpClientFactory _httpClientFactory) : ITempDrugUploadApplicationService
{
    public async Task<ErrorOr<CreatedImportBatchResponse>> UploadFromUrlAsync(string url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Error.Validation(description: "Url is required.");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? parsedUrl) || parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps)
        {
            return Error.Validation(description: "Url must be a valid absolute HTTP or HTTPS URL.");
        }

        HttpClient client = _httpClientFactory.CreateClient("ImportDownload");
        using HttpRequestMessage request = new(HttpMethod.Get, parsedUrl);
        using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Error.Validation(
                description: "Failed to download from the provided URL.",
                metadata: new()
                {
                    ["statusCode"] = (int)response.StatusCode,
                    ["resourceType"] = "ImportFileUrl"
                });
        }

        string? contentType = response.Content.Headers.ContentType?.MediaType;
        string fileName = ResolveFileName(response, parsedUrl);
        if (!IsExcelFile(fileName, contentType))
        {
            return Error.Validation(description: "Only .xlsx files are supported.");
        }

        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var memoryStream = new MemoryStream();
        await responseStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return await AnalyzeAndStoreAsync(
            stream: memoryStream,
            fileName: fileName,
            sourceType: parsedUrl.Host,
            sourcePath: parsedUrl.ToString(),
            contentType: string.IsNullOrWhiteSpace(contentType)
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : contentType!,
            cancellationToken: cancellationToken);
    }

    public async Task<ErrorOr<CreatedImportBatchResponse>> UploadFromFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Error.Validation(description: "Uploaded file is required.");
        }

        if (!IsExcelFile(file.FileName, file.ContentType))
        {
            return Error.Validation(description: "Only .xlsx files are supported.");
        }

        await using Stream fileStream = file.OpenReadStream();
        await using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        string contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : file.ContentType;

        return await AnalyzeAndStoreAsync(
            stream: memoryStream,
            fileName: file.FileName,
            sourceType: "file",
            sourcePath: null,
            contentType: contentType,
            cancellationToken: cancellationToken);
    }

    private async Task<ErrorOr<CreatedImportBatchResponse>> AnalyzeAndStoreAsync(
        MemoryStream stream,
        string fileName,
        string sourceType,
        string? sourcePath,
        string contentType,
        CancellationToken cancellationToken)
    {
        // Upload the file to the blob storage
        stream.Position = 0;
        Guid blobFileId = await _blobService.UploadAsync(stream, contentType, cancellationToken);

        // Analyze the workbook
        stream.Position = 0;
        ErrorOr<(List<string> Headers, int TotalRows)> analysisResult = AnalyzeWorkbook(stream);
        if (analysisResult.IsError)
        {
            return analysisResult.Errors;
        }

        (List<string> headers, int totalRows) = analysisResult.Value;

        // Create the import batch
        ErrorOr<TempDrugImportBatch> batchResult = TempDrugImportBatch.Create(
            nameOfFile: fileName,
            blobFileId: blobFileId,
            sourceType: sourceType,
            sourcePath: sourcePath,
            totalQuantityCount: totalRows);
        if (batchResult.IsError)
        {
            return batchResult.Errors;
        }

        // Add the import batch to the repository
        TempDrugImportBatch batch = batchResult.Value;
        await _batchRepository.AddAsync(batch, cancellationToken);

        // Get the default mappings
        List<DefaultMappingForColumnsAttribute> defaults = await _defaultMappingRepository.GetAllAsync(cancellationToken);
        Dictionary<string, string> defaultMapping = BuildDefaultMapping(headers, defaults);

        return batch.MapToCreatedResponse(headers, defaultMapping);
    }

    private static ErrorOr<(List<string> Headers, int TotalRows)> AnalyzeWorkbook(Stream stream)
    {
        try
        {
            using var workbook = new XLWorkbook(stream);
            IXLWorksheet worksheet = workbook.Worksheets.First();
            IXLRange? usedRange = worksheet.RangeUsed();

            if (usedRange is null)
            {
                return (new List<string>(), 0);
            }

            var headers = usedRange.FirstRow()
                .Cells()
                .Select(cell => cell.GetString().Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            int totalRows = Math.Max(usedRange.RowCount() - 1, 0);
            return (headers, totalRows);
        }
        catch (Exception)
        {
            return Error.Validation(description: "Uploaded file could not be parsed as a valid .xlsx document.");
        }
    }

    private static Dictionary<string, string> BuildDefaultMapping(
        List<string> headers,
        List<DefaultMappingForColumnsAttribute> defaults)
    {
        var headerLookup = headers
            .Where(header => !string.IsNullOrWhiteSpace(header))
            .ToDictionary(header => header, header => header, StringComparer.OrdinalIgnoreCase);

        var defaultsByField = defaults
            .GroupBy(defaultMap => defaultMap.MappedFieldName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(defaultMap => defaultMap.SourceColumnName).ToList(),
                StringComparer.OrdinalIgnoreCase);

        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string fieldName in TempDrugFields.All)
        {
            mapping[fieldName] = string.Empty;

            if (!defaultsByField.TryGetValue(fieldName, out List<string>? sourceCandidates))
            {
                continue;
            }

            string? matchedHeader = sourceCandidates
                .FirstOrDefault(candidate => headerLookup.ContainsKey(candidate));

            if (!string.IsNullOrWhiteSpace(matchedHeader))
            {
                mapping[fieldName] = headerLookup[matchedHeader];
            }
        }

        return mapping;
    }

    private static bool IsExcelFile(string fileName, string? contentType)
    {
        bool isXlsxName = fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
        if (isXlsxName)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType.Contains("spreadsheetml", StringComparison.OrdinalIgnoreCase)
               || contentType.Contains("excel", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveFileName(HttpResponseMessage response, Uri parsedUrl)
    {
        string? fileNameFromHeader = response.Content.Headers.ContentDisposition?.FileNameStar
                                     ?? response.Content.Headers.ContentDisposition?.FileName;
        if (!string.IsNullOrWhiteSpace(fileNameFromHeader))
        {
            return fileNameFromHeader.Trim('"');
        }

        string fromPath = Path.GetFileName(parsedUrl.AbsolutePath);
        if (!string.IsNullOrWhiteSpace(fromPath))
        {
            return fromPath;
        }

        return "import.xlsx";
    }
}

