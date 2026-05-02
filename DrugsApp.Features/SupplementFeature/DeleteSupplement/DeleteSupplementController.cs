using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.DeleteSupplement;

/// <summary>
/// Command for deleting a supplement.
/// </summary>
public sealed record DeleteSupplementCommand(Guid SupplementId) : IRequest<ErrorOr<DeleteSupplementResponse>>;

/// <summary>
/// Response after deleting a supplement.
/// </summary>
public sealed record DeleteSupplementResponse(SupplementDto Supplement);


/// <summary>
/// Controller for deleting supplements.
/// </summary>
[ApiController]
[Tags("Supplements")]
[Route("api/supplements")]
public sealed class DeleteSupplementController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Route name for delete supplement.
    /// </summary>
    public const string RouteName = "DeleteSupplement";

    /// <summary>
    /// Deletes a supplement when not referenced by groups.
    /// </summary>
    /// <param name="id">Supplement identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns deleted supplement id.</response>
    /// <response code="404">Supplement not found.</response>
    /// <response code="409">Supplement referenced by groups.</response>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(DeleteSupplementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DeleteSupplementResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        DeleteSupplementCommand command = DeleteSupplementMapping.MapToCommand(id);
        ErrorOr<DeleteSupplementResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
