using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using DrugsApp.Features.ActiveSubstanceFeature.UpdateActiveSubstance;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance;


/// <summary>
/// Request model for adding an active substance.
/// </summary>
public sealed record AddActiveSubstanceRequest(
    string ActiveSubstanceName,
    bool? HasAlcoholInteraction,
    string? AlcoholSpecialText,
    AddOralMedicationIntakeGuidelinesRequest? OralMedicationIntakeGuidelines);

/// <summary>
/// Response containing created active substance details.
/// </summary>
public sealed record CreatedActiveSubstanceResponse(
    Guid Id,
    string ActiveSubstanceName,
    bool HasAlcoholInteraction,
    string? AlcoholSpecialText,
    OralMedicationIntakeGuidelinesDto? OralMedicationIntakeGuidelines);

/// <summary>
/// Command for adding a new active substance.
/// </summary>
public sealed record AddActiveSubstanceCommand(
    AddActiveSubstanceRequest Request) : IRequest<ErrorOr<CreatedActiveSubstanceResponse>>;

/// <summary>
/// Controller for adding active substances.
/// </summary>
[ApiController]
[Tags("Active Substances")]
[Route("api/active-substances")]
public sealed class AddActiveSubstanceController(
    IMediator _mediator,
    IValidator<AddActiveSubstanceRequest> _validator) : ControllerBase
{
    public const string RouteName = "AddActiveSubstance";

    /// <summary>
    /// Creates a new active substance.
    /// </summary>
    /// <param name="request">Create request containing active substance name and optional guidelines.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns the created active substance.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="409">An active substance with the same name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreatedActiveSubstanceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedActiveSubstanceResponse>> Create(
        [FromBody] AddActiveSubstanceRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddActiveSubstanceCommand command = request.MapToCommand();
        ErrorOr<CreatedActiveSubstanceResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            created => CreatedAtRoute(
                routeName: UpdateActiveSubstanceController.RouteName,
                routeValues: new { id = created.Id },
                value: created),
            errors => errors.MapToProblem(HttpContext));
    }
}


