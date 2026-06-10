using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblemType;

/// <summary>
/// Request model for updating a drug problem type.
/// </summary>
public sealed record UpdateDrugProblemTypeRequest(string? ProblemTypeName);

/// <summary>
/// Command for updating a drug problem type.
/// </summary>
public sealed record UpdateDrugProblemTypeCommand(Guid Id, UpdateDrugProblemTypeRequest Request) : IRequest<ErrorOr<DrugProblemTypeDto>>;

/// <summary>
/// Controller for updating drug problem types.
/// </summary>
[ApiController]
[Authorize(Policy = "ManageOther")]
[Tags("Drug Problem Types")]
[Route("api/drug-problem-types")]
public sealed class UpdateDrugProblemTypeController(
    IMediator _mediator,
    IValidator<UpdateDrugProblemTypeRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating a drug problem type.
    /// </summary>
    public const string RouteName = "UpdateDrugProblemType";

    /// <summary>
    /// Updates a drug problem type entry.
    /// </summary>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(DrugProblemTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DrugProblemTypeDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDrugProblemTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return ValidationProblem("id must not be empty.");
        }

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateDrugProblemTypeCommand command = request.MapToCommand(id);
        ErrorOr<DrugProblemTypeDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<DrugProblemTypeDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
