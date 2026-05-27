using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.InteractionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.InteractionFeature.UpdateInteraction;

/// <summary>
/// Request payload for updating an interaction.
/// </summary>
public sealed record UpdateInteractionRequest(
    string? DefaultText,
    string? StomachText,
    Guid? SeverityId,
    bool? ClearDefaultText,
    bool? ClearStomachText);

/// <summary>
/// Controller for updating interactions.
/// </summary>
[ApiController]
[Tags("ActiveSubstanceInteractions")]
[Route("api/active-substance-interactions")]
public sealed class UpdateInteractionController(
    IMediator mediator,
    IValidator<UpdateInteractionRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateInteraction";

    /// <summary>
    /// Updates interaction text or severity.
    /// </summary>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedInteractionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdatedInteractionResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateInteractionRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateInteractionCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedInteractionResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
