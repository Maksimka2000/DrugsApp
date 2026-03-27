using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.RevertImport;

/// <summary>
/// Response returned when revert execution is accepted.
/// </summary>
public sealed record RevertImportResponse(
    Guid BatchId,
    string Status);

/// <summary>
/// Command for starting revert execution.
/// </summary>
public sealed record RevertImportCommand(Guid BatchId) : IRequest<ErrorOr<RevertImportResponse>>;

/// <summary>
/// Controller for revert import workflow.
/// </summary>
[ApiController]
[Tags("TempDrugImport")]
[Route("api/temp-drug-import-batches")]
public sealed class RevertImportController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Starts revert operation for selected import batch.
    /// </summary>
    [HttpPost("{id:guid}/revert")]
    public async Task<ActionResult<RevertImportResponse>> Revert(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        RevertImportCommand command = RevertImportMapping.MapToCommand(id);
        ErrorOr<RevertImportResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            response => Accepted(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

