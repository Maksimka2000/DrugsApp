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

namespace DrugsApp.Features.AllergiesFeature.UpdateOtherAllergy;

public sealed record UpdateOtherAllergyRequest(string? AllergyName, string? Note, bool? ClearNote);
public sealed record UpdateOtherAllergyCommand(Guid UserId, Guid Id, UpdateOtherAllergyRequest Request)
    : IRequest<ErrorOr<UpdatedOtherAllergyResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class UpdateOtherAllergyController(
    IMediator _mediator,
    IValidator<UpdateOtherAllergyRequest> _validator) : ControllerBase
{
    public const string RouteName = "UpdateOtherAllergy";

    [HttpPatch("other-allergies/{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedOtherAllergyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedOtherAllergyResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateOtherAllergyRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return ValidationProblem("id must not be empty.");
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId)) return Unauthorized();

        UpdateOtherAllergyCommand command = request.MapToCommand(userId, id);
        ErrorOr<UpdatedOtherAllergyResponse> result = await _mediator.Send(command, cancellationToken);
        return result.Match<ActionResult<UpdatedOtherAllergyResponse>>(response => Ok(response), errors => errors.MapToProblem(HttpContext));
    }
}
