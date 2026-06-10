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

namespace DrugsApp.Features.AllergiesFeature.CreateSupplementAllergy;

public sealed record CreateSupplementAllergyRequest(
    Guid? SupplementId,
    string? CustomSupplementName,
    string? Note,
    string Source);

public sealed record CreateSupplementAllergyCommand(
    Guid UserId,
    CreateSupplementAllergyRequest Request) : IRequest<ErrorOr<CreatedSupplementAllergyResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class CreateSupplementAllergyController(
    IMediator _mediator,
    IValidator<CreateSupplementAllergyRequest> _validator) : ControllerBase
{
    public const string RouteName = "CreateSupplementAllergy";

    [HttpPost("supplements", Name = RouteName)]
    [ProducesResponseType(typeof(CreatedSupplementAllergyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedSupplementAllergyResponse>> Create(
        [FromBody] CreateSupplementAllergyRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
            return Unauthorized();

        CreateSupplementAllergyCommand command = request.MapToCommand(userId);
        ErrorOr<CreatedSupplementAllergyResponse> result = await _mediator.Send(command, cancellationToken);
        return result.Match<ActionResult<CreatedSupplementAllergyResponse>>(
            response => CreatedAtRoute(
                routeName: UpdateSupplementAllergyNote.UpdateSupplementAllergyNoteController.RouteName,
                routeValues: new { id = response.Item.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
