using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.UploadImportViaFile;

/// <summary>
/// Request model for file-based import upload.
/// </summary>
public sealed record UploadImportViaFileRequest(IFormFile File);

/// <summary>
/// Command for file-based import upload.
/// </summary>
public sealed record UploadImportViaFileCommand(
    UploadImportViaFileRequest Request) : IRequest<ErrorOr<CreatedImportBatchResponse>>;

/// <summary>
/// Controller for file import upload workflow.
/// </summary>
[ApiController]
[Tags("TempDrugImport")]
[Route("api/temp-drug-imports")]
public sealed class UploadImportViaFileController(
    IMediator _mediator,
    IValidator<UploadImportViaFileRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Uploads import file and returns analyzed mapping payload.
    /// </summary>
    [HttpPost("file-upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CreatedImportBatchResponse>> UploadFromFile(
        [FromForm] UploadImportViaFileRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UploadImportViaFileCommand command = request.MapToCommand();
        ErrorOr<CreatedImportBatchResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            response => CreatedAtRoute(
                routeName: "GetImportBatch",
                routeValues: new { id = response.BatchId },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}

