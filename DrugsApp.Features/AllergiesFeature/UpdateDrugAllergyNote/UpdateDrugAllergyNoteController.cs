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

namespace DrugsApp.Features.AllergiesFeature.UpdateDrugAllergyNote;

/// <summary>
/// Command for updating a drug allergy note.
/// </summary>
public sealed record UpdateDrugAllergyNoteCommand(
    Guid UserId,
    Guid Id,
    UpdateAllergyNoteRequest Request) : IRequest<ErrorOr<UpdatedDrugAllergyResponse>>;

/// <summary>
/// Controller for updating drug allergy notes.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class UpdateDrugAllergyNoteController(
    IMediator _mediator,
    IValidator<UpdateAllergyNoteRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating a drug allergy note.
    /// </summary>
    public const string RouteName = "UpdateDrugAllergyNote";

    /// <summary>
    /// Updates the note on a drug allergy owned by the current user.
    /// </summary>
    [HttpPatch("drugs/{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedDrugAllergyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdatedDrugAllergyResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateAllergyNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return ValidationProblem("id must not be empty.");
        }

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        UpdateDrugAllergyNoteCommand command = request.MapToCommand(userId, id);
        ErrorOr<UpdatedDrugAllergyResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UpdatedDrugAllergyResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
