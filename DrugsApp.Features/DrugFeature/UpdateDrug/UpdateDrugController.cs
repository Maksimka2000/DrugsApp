using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugFeature.UpdateDrug;

/// <summary>
/// Request model for partially updating drug fields.
/// </summary>
public sealed record UpdateDrugRequest(
    string? CommercialName,
    List<Guid>? NewActiveSubstanceIds,
    DrugType? TypeOfDrug);

/// <summary>
/// Data transfer object representing an active substance summary.
/// </summary>
public sealed record ActiveSubstanceSummaryDto(Guid Id, string Name);

/// <summary>
/// Response containing updated drug state required by the client.
/// </summary>
public sealed record UpdatedDrugResponse(
    Guid Id,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    List<ActiveSubstanceSummaryDto> ActiveSubstances);

/// <summary>
/// Command for partially updating a drug.
/// </summary>
public sealed record UpdateDrugCommand(
    Guid DrugId,
    UpdateDrugRequest Request) : IRequest<ErrorOr<UpdatedDrugResponse>>;

/// <summary>
/// Controller for updating drug information.
/// </summary>
[ApiController]
[Tags("Drugs")]
[Route("api/drugs")]
public sealed class UpdateDrugController(
    IMediator mediator,
    IValidator<UpdateDrugRequest> validator) : ControllerBase
{
    public const string RouteName = "UpdateDrug";

    private readonly IMediator _mediator = mediator;
    private readonly IValidator<UpdateDrugRequest> _validator = validator;

    /// <summary>
    /// Partially updates a drug's commercial name, type, and active substances set.
    /// </summary>
    /// <param name="id">Drug unique identifier.</param>
    /// <param name="request">PATCH payload with fields to change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated drug information.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Drug not found.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedDrugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdatedDrugResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDrugRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateDrugCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedDrugResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

