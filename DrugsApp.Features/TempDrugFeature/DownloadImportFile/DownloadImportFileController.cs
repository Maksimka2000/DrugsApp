using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.DownloadImportFile;

/// <summary>
/// Query model for downloading originally uploaded import file.
/// </summary>
public sealed record DownloadImportFileQuery(Guid BatchId) : IRequest<ErrorOr<DownloadImportFileResponse>>;

/// <summary>
/// File payload returned for import file download.
/// </summary>
public sealed record DownloadImportFileResponse(
    Stream Stream,
    string ContentType,
    string FileName);

/// <summary>
/// Controller for downloading uploaded import files.
/// </summary>
[ApiController]
[Tags("TempDrugImport")]
[Route("api/temp-drug-imports")]
public sealed class DownloadImportFileController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Streams originally uploaded file for selected import batch.
    /// </summary>
    [HttpGet("{id:guid}/file")]
    public async Task<ActionResult> Download(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        DownloadImportFileQuery query = DownloadImportFileMapping.MapToQuery(id);
        ErrorOr<DownloadImportFileResponse> response = await _mediator.Send(query, cancellationToken);

        return response.Match<ActionResult>(
            file => File(file.Stream, file.ContentType, file.FileName),
            errors => errors.MapToProblem(HttpContext));
    }
}

