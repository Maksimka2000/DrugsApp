using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ActiveSubstanceFeature.UpdateActiveSubstance;

/// <summary>
/// Request model for partially updating active substance fields.
/// </summary>
public sealed record UpdateActiveSubstanceRequest(
    string? ActiveSubstanceName,
    bool? HasAlcoholInteraction,
    string? AlcoholSpecialText,
    bool? ClearAlcoholSpecialText,
    FoodIntakeType? FoodIntakeType,
    int? WaitBeforeMinutes,
    bool? ClearWaitBeforeMinutes,
    int? WaitAfterMinutes,
    bool? ClearWaitAfterMinutes,
    string? MealNotes,
    bool? ClearMealNotes,
    string? AvoidFoodDrug,
    bool? ClearAvoidFoodDrug,
    string? AvoidFoodTherapy,
    bool? ClearAvoidFoodTherapy,
    string? LiquidNotes,
    bool? ClearLiquidNotes,
    bool? LiquidInstructions,
    bool? ClearLiquidInstructions,
    bool? RecommendedMorning,
    bool? ClearRecommendedMorning,
    bool? RecommendedEvening,
    bool? ClearRecommendedEvening);

/// <summary>
/// Response containing updated active substance fields.
/// </summary>
public sealed record UpdatedActiveSubstanceResponse(
    Guid Id,
    string ActiveSubstanceName,
    bool HasAlcoholInteraction,
    string? AlcoholSpecialText,
    OralMedicationIntakeGuidelinesDto? OralMedicationIntakeGuidelines);

/// <summary>
/// Command for partially updating an active substance.
/// </summary>
public sealed record UpdateActiveSubstanceCommand(
    Guid ActiveSubstanceId,
    UpdateActiveSubstanceRequest Request) : IRequest<ErrorOr<UpdatedActiveSubstanceResponse>>;

/// <summary>
/// Controller for updating active substance information.
/// </summary>
[ApiController]
[Tags("Active Substances")]
[Route("api/active-substances")]
public sealed class UpdateActiveSubstanceController(
    IMediator _mediator,
    IValidator<UpdateActiveSubstanceRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateActiveSubstance";

    /// <summary>
    /// Partially updates the active substance and optional oral guidelines fields.
    /// </summary>
    /// <param name="id">Active substance unique identifier.</param>
    /// <param name="request">PATCH payload with fields to change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated active substance information.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Active substance not found.</response>
    /// <response code="409">Conflict due to duplicates or invalid state.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedActiveSubstanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedActiveSubstanceResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateActiveSubstanceRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateActiveSubstanceCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedActiveSubstanceResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}


