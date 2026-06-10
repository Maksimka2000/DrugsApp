using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularSupplementsFeature.DeleteOwnSupplement;

/// <summary>
/// Command for deleting an own supplement compound with cascade removal.
/// </summary>
public sealed record DeleteOwnSupplementCommand(Guid UserId, Guid Id)
    : IRequest<ErrorOr<DeleteOwnSupplementResponse>>;

/// <summary>
/// Controller for deleting own supplement compounds.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Own Supplements")]
[Route("api/own-supplements")]
public sealed class DeleteOwnSupplementController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for deleting an own supplement compound.
    /// </summary>
    public const string RouteName = "DeleteOwnSupplement";

    /// <summary>
    /// Deletes an own supplement compound and cascade-removes referencing regular supplements.
    /// </summary>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(DeleteOwnSupplementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteOwnSupplementResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return ValidationProblem("id must not be empty.");
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        DeleteOwnSupplementCommand command = new(userId, id);
        ErrorOr<DeleteOwnSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<DeleteOwnSupplementResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
