using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DrugsApp.Features.SupplementFeature.Shared;

namespace DrugsApp.Features.SupplementFeature.DeleteSupplementGroup;



/// <summary>
/// Command for deleting a supplement group.
/// </summary>
public sealed record DeleteSupplementGroupCommand(Guid SupplementGroupId) : IRequest<ErrorOr<DeleteSupplementGroupResponse>>;

/// <summary>
/// Response after deleting a supplement group.
/// </summary>
public sealed record DeleteSupplementGroupResponse(SupplementGroupDto SupplementGroup);

/// <summary>
/// Controller for deleting supplement groups.
/// </summary>
[ApiController]
[Tags("SupplementGroups")]
[Route("api/supplement-groups")]
public sealed class DeleteSupplementGroupController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Route name for delete supplement group.
    /// </summary>
    public const string RouteName = "DeleteSupplementGroup";

    /// <summary>
    /// Deletes a supplement group when safe.
    /// </summary>
    /// <param name="id">Supplement group identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns deleted group id.</response>
    /// <response code="404">Group not found.</response>
    /// <response code="409">Group referenced by other data.</response>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(DeleteSupplementGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DeleteSupplementGroupResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        DeleteSupplementGroupCommand command = DeleteSupplementGroupMapping.MapToCommand(id);
        ErrorOr<DeleteSupplementGroupResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
