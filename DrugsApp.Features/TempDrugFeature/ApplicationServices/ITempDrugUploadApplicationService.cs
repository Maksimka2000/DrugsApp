using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.TempDrugFeature.ApplicationServices;

/// <summary>
/// Upload orchestration service for URL/file import entrypoints.
/// </summary>
public interface ITempDrugUploadApplicationService
{
    /// <summary>
    /// Downloads, validates, stores, and analyzes import file from URL.
    /// </summary>
    Task<ErrorOr<CreatedImportBatchResponse>> UploadFromUrlAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Validates, stores, and analyzes uploaded import file.
    /// </summary>
    Task<ErrorOr<CreatedImportBatchResponse>> UploadFromFileAsync(IFormFile file, CancellationToken cancellationToken);
}

