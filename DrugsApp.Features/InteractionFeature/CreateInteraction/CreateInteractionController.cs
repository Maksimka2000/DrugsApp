using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.InteractionFeature.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ErrorOr;

namespace DrugsApp.Features.InteractionFeature.CreateInteraction;


/// <summary>
/// Command for creating an interaction.
/// </summary>
public sealed record CreateInteractionCommand(CreateInteractionRequest Request)
    : IRequest<ErrorOr<CreatedInteractionResponse>>;

/// <summary>
/// Response containing created interaction details.
/// </summary>
public sealed record CreatedInteractionResponse(
    Guid InteractionId,
    InteractionQueryFoundDto Left,
    InteractionQueryFoundDto Right,
    InteractionSeverityDto Severity,
    string? DefaultText,
    string? StomachText);

/// <summary>
/// Interaction side payload.
/// </summary>
public sealed record InteractionSideRequest(
    InteractionType Type,
    Guid Id);

/// <summary>
/// Request payload for creating an interaction.
/// </summary>
public sealed record CreateInteractionRequest(
    InteractionSideRequest Left,
    InteractionSideRequest Right,
    Guid SeverityId,
    string? DefaultText,
    string? StomachText);

/// <summary>
/// Controller for creating interactions.
/// </summary>
[ApiController]
[Tags("ActiveSubstanceInteractions")]
[Route("api/active-substance-interactions")]
public sealed class CreateInteractionController(
    IMediator mediator,
    IValidator<CreateInteractionRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "CreateInteraction";

    /// <summary>
    /// Creates a new interaction.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreatedInteractionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedInteractionResponse>> Create(
        [FromBody] CreateInteractionRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateInteractionCommand command = request.MapToCommand();
        ErrorOr<CreatedInteractionResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtRoute(
                routeName: UpdateInteraction.UpdateInteractionController.RouteName,
                routeValues: new { id = response.InteractionId },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
