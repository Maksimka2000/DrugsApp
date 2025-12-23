using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.VetDrugFeature.UpdateVetDrug;


/// <summary>
/// Request model for updating veterinary drug fields.
/// </summary>
public sealed record UpdateVetDrugRequest(
    string? CommercialName);

/// <summary>
/// Response containing updated veterinary drug fields.
/// </summary>
public sealed record UpdatedVetDrugResponse(
    Guid Id,
    string CommercialName);

/// <summary>
/// Command for updating a veterinary drug's commercial name.
/// </summary>
public sealed record UpdateVetDrugCommand(
    Guid VetDrugId,
    UpdateVetDrugRequest Request) : IRequest<ErrorOr<UpdatedVetDrugResponse>>;




/// <summary>
/// Controller for updating veterinary drug information.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UpdateVetDrugController"/> class.
/// </remarks>
/// <param name="mediator">MediatR mediator.</param>
/// <param name="validator">Validator for the request.</param>
[ApiController]
[Tags("Vet Drugs")]
[Route("api/vet-drugs")]
public sealed class UpdateVetDrugController(
    IMediator mediator,
    IValidator<UpdateVetDrugRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateVetDrug";

    private readonly IMediator _mediator = mediator;
    private readonly IValidator<UpdateVetDrugRequest> _validator = validator;

    /// <summary>
    /// Updates the commercial name of a veterinary drug.
    /// </summary>
    /// <param name="id">Veterinary drug unique identifier.</param>
    /// <param name="request">Update request containing new commercial name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated veterinary drug information.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Veterinary drug not found.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedVetDrugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdatedVetDrugResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateVetDrugRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateVetDrugCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedVetDrugResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext)
        );
    }
}

