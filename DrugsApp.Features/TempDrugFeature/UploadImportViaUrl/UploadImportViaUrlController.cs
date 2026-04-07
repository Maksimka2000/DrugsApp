using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.UploadImportViaUrl;

/// <summary>
/// Request model for URL-based import upload.
/// </summary>
public sealed record UploadImportViaUrlRequest(string Url);

/// <summary>
/// Command for URL-based import upload.
/// </summary>
public sealed record UploadImportViaUrlCommand(
    UploadImportViaUrlRequest Request) : IRequest<ErrorOr<CreatedImportBatchResponse>>;

/// <summary>
/// Controller for URL import upload workflow.
/// </summary>
[ApiController]
[Tags("TempDrugImport")]
[Route("api/temp-drug-imports")]
public sealed class UploadImportViaUrlController(
    IMediator _mediator,
    IValidator<UploadImportViaUrlRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Uploads import file from URL and returns analyzed mapping payload.
    /// </summary>
    [HttpPost("url-upload")]
    public async Task<ActionResult<CreatedImportBatchResponse>> UploadFromUrl(
        [FromBody] UploadImportViaUrlRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UploadImportViaUrlCommand command = request.MapToCommand();
        ErrorOr<CreatedImportBatchResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            response => CreatedAtRoute(
                routeName: "GetImportBatch",
                routeValues: new { id = response.BatchId },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}

