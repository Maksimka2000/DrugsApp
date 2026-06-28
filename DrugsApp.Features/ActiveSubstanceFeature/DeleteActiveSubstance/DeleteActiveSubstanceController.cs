using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ActiveSubstanceFeature.DeleteActiveSubstance;

/// <summary>
/// Command for deleting an active substance.
/// </summary>
public sealed record DeleteActiveSubstanceCommand(Guid ActiveSubstanceId)
    : IRequest<ErrorOr<DeleteActiveSubstanceResponse>>;

/// <summary>
/// Response after deleting an active substance.
/// </summary>
public sealed record DeleteActiveSubstanceResponse(ActiveSubstanceDto ActiveSubstance);

/// <summary>
/// Controller for deleting active substances.
/// </summary>
[ApiController]
[Tags("Active Substances")]
[Route("api/active-substances")]
public sealed class DeleteActiveSubstanceController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Route name for delete active substance.
    /// </summary>
    public const string RouteName = "DeleteActiveSubstance";

    /// <summary>
    /// Deletes an active substance when it is not included in any drug.
    /// </summary>
    /// <param name="id">Active substance identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the deleted active substance.</response>
    /// <response code="404">Active substance not found.</response>
    /// <response code="409">Active substance is included in one or more drugs.</response>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(DeleteActiveSubstanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DeleteActiveSubstanceResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        DeleteActiveSubstanceCommand command = DeleteActiveSubstanceMapping.MapToCommand(id);
        ErrorOr<DeleteActiveSubstanceResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
