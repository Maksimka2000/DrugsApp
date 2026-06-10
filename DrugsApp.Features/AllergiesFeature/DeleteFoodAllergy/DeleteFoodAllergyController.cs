using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AllergiesFeature.DeleteFoodAllergy;

public sealed record DeleteFoodAllergyCommand(Guid UserId, Guid Id) : IRequest<ErrorOr<Success>>;

[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class DeleteFoodAllergyController(IMediator _mediator) : ControllerBase
{
    public const string RouteName = "DeleteFoodAllergy";

    [HttpDelete("foods/{id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return ValidationProblem("id must not be empty.");
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId)) return Unauthorized();

        ErrorOr<Success> result = await _mediator.Send(new DeleteFoodAllergyCommand(userId, id), cancellationToken);
        return result.Match<ActionResult>(_ => NoContent(), errors => errors.MapToProblem(HttpContext));
    }
}
