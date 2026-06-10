using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularDrugsFeature.GetRegularDrug;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;

/// <summary>
/// Request model for creating a regular drug schedule.
/// </summary>
public sealed record CreateRegularDrugRequest(
    Guid DrugId,
    Guid DrugFormId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

/// <summary>
/// Command for creating a regular drug schedule.
/// </summary>
public sealed record CreateRegularDrugCommand(
    Guid UserId,
    CreateRegularDrugRequest Request) : IRequest<ErrorOr<CreateRegularDrugResponse>>;

/// <summary>
/// Response containing the created regular drug schedule.
/// </summary>
public sealed record CreateRegularDrugResponse(RegularDrugListItemDto Item);

/// <summary>
/// Controller for creating regular drug schedules.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Regular Drugs")]
[Route("api/regular-drugs")]
public sealed class CreateRegularDrugController(
    IMediator _mediator,
    IValidator<CreateRegularDrugRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating a regular drug schedule.
    /// </summary>
    public const string RouteName = "CreateRegularDrug";

    /// <summary>
    /// Creates a new regular drug schedule for the authenticated user.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreateRegularDrugResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateRegularDrugResponse>> Create(
        [FromBody] CreateRegularDrugRequest request,
        CancellationToken cancellationToken = default)
    {
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

        CreateRegularDrugCommand command = request.MapToCommand(userId);
        ErrorOr<CreateRegularDrugResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CreateRegularDrugResponse>>(
            response => CreatedAtRoute(
                GetRegularDrugController.RouteName,
                new { id = response.Item.Id },
                response),
            errors => errors.MapToProblem(HttpContext));
    }
}
