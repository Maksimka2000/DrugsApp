using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.CancelImport;

/// <summary>
/// Response returned after cancelling an import or revert batch.
/// </summary>
public sealed record CancelImportResponse(
    Guid BatchId,
    string Status);

/// <summary>
/// Command for cancelling import or revert execution.
/// </summary>
public sealed record CancelImportCommand(Guid BatchId) : IRequest<ErrorOr<CancelImportResponse>>;

/// <summary>
/// Controller for cancellation workflow of import/revert jobs.
/// </summary>
[ApiController]
[Tags("TempDrugImport")]
[Route("api/temp-drug-imports")]
public sealed class CancelImportController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Cancels running import or revert execution.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<CancelImportResponse>> Cancel(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        CancelImportCommand command = CancelImportMapping.MapToCommand(id);
        ErrorOr<CancelImportResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

