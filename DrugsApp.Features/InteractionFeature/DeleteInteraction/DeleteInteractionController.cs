using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.InteractionFeature.DeleteInteraction;

/// <summary>
/// Controller for deleting interactions.
/// </summary>
[ApiController]
[Tags("ActiveSubstanceInteractions")]
[Route("api/active-substance-interactions")]
public sealed class DeleteInteractionController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "DeleteInteraction";

    /// <summary>
    /// Deletes an interaction.
    /// </summary>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        ErrorOr<Success> result = await mediator.Send(new DeleteInteractionCommand(id), cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
