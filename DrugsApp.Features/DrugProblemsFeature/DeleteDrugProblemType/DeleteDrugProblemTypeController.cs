using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugProblemsFeature.DeleteDrugProblemType;

/// <summary>
/// Command for deleting a drug problem type.
/// </summary>
public sealed record DeleteDrugProblemTypeCommand(Guid Id) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Controller for deleting drug problem types.
/// </summary>
[ApiController]
[Authorize(Policy = "ManageOther")]
[Tags("Drug Problem Types")]
[Route("api/drug-problem-types")]
public sealed class DeleteDrugProblemTypeController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name for deleting a drug problem type.
    /// </summary>
    public const string RouteName = "DeleteDrugProblemType";

    /// <summary>
    /// Deletes a drug problem type entry.
    /// </summary>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return ValidationProblem("id must not be empty.");
        }

        DeleteDrugProblemTypeCommand command = new(id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
