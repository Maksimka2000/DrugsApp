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

namespace DrugsApp.Features.AllergiesFeature.CreateFoodAllergy;

public sealed record CreateFoodAllergyRequest(
    Guid? FoodId,
    string? CustomFoodName,
    string? Note,
    string Source);

public sealed record CreateFoodAllergyCommand(
    Guid UserId,
    CreateFoodAllergyRequest Request) : IRequest<ErrorOr<CreatedFoodAllergyResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class CreateFoodAllergyController(
    IMediator _mediator,
    IValidator<CreateFoodAllergyRequest> _validator) : ControllerBase
{
    public const string RouteName = "CreateFoodAllergy";

    [HttpPost("foods", Name = RouteName)]
    [ProducesResponseType(typeof(CreatedFoodAllergyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedFoodAllergyResponse>> Create(
        [FromBody] CreateFoodAllergyRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
            return Unauthorized();

        CreateFoodAllergyCommand command = request.MapToCommand(userId);
        ErrorOr<CreatedFoodAllergyResponse> result = await _mediator.Send(command, cancellationToken);
        return result.Match<ActionResult<CreatedFoodAllergyResponse>>(
            response => CreatedAtRoute(
                routeName: UpdateFoodAllergyNote.UpdateFoodAllergyNoteController.RouteName,
                routeValues: new { id = response.Item.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
