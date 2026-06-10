using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AllergiesFeature.UpdateFoodAllergyNote;

public sealed record UpdateFoodAllergyNoteCommand(
    Guid UserId, Guid Id, UpdateAllergyNoteRequest Request) : IRequest<ErrorOr<UpdatedFoodAllergyResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class UpdateFoodAllergyNoteController(
    IMediator _mediator,
    IValidator<UpdateAllergyNoteRequest> _validator) : ControllerBase
{
    public const string RouteName = "UpdateFoodAllergyNote";

    [HttpPatch("foods/{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedFoodAllergyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdatedFoodAllergyResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateAllergyNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return ValidationProblem("id must not be empty.");
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId)) return Unauthorized();

        UpdateFoodAllergyNoteCommand command = request.MapToCommand(userId, id);
        ErrorOr<UpdatedFoodAllergyResponse> result = await _mediator.Send(command, cancellationToken);
        return result.Match<ActionResult<UpdatedFoodAllergyResponse>>(response => Ok(response), errors => errors.MapToProblem(HttpContext));
    }
}
