using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature.Enums;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthConditionFeature.RemoveHealthCondition;

/// <summary>
/// Command for removing a health condition.
/// </summary>
public sealed record RemoveHealthConditionCommand(Guid Id) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Controller for removing catalog health conditions.
/// </summary>
[ApiController]
[Authorize(Policy = nameof(Permission.ManageOther))]
[Tags("Health Conditions")]
[Route("api/health-conditions")]
public sealed class RemoveHealthConditionController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for removing a health condition.
    /// </summary>
    public const string RouteName = "RemoveHealthCondition";

    /// <summary>
    /// Removes a catalog health condition.
    /// </summary>
    /// <param name="id">Health condition identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Successfully removed.</response>
    /// <response code="404">Health condition not found.</response>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Remove(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        RemoveHealthConditionCommand command = new(id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
