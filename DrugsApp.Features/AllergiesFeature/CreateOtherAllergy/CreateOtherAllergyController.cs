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

namespace DrugsApp.Features.AllergiesFeature.CreateOtherAllergy;

public sealed record CreateOtherAllergyRequest(string AllergyName, string? Note);
public sealed record CreateOtherAllergyCommand(Guid UserId, CreateOtherAllergyRequest Request)
    : IRequest<ErrorOr<CreatedOtherAllergyResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class CreateOtherAllergyController(
    IMediator _mediator,
    IValidator<CreateOtherAllergyRequest> _validator) : ControllerBase
{
    public const string RouteName = "CreateOtherAllergy";

    [HttpPost("other-allergies", Name = RouteName)]
    [ProducesResponseType(typeof(CreatedOtherAllergyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedOtherAllergyResponse>> Create(
        [FromBody] CreateOtherAllergyRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId)) return Unauthorized();

        CreateOtherAllergyCommand command = request.MapToCommand(userId);
        ErrorOr<CreatedOtherAllergyResponse> result = await _mediator.Send(command, cancellationToken);
        return result.Match<ActionResult<CreatedOtherAllergyResponse>>(
            response => CreatedAtRoute(
                routeName: UpdateOtherAllergy.UpdateOtherAllergyController.RouteName,
                routeValues: new { id = response.Item.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
