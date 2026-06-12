using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblem;

/// <summary>
/// Request model for updating a drug problem.
/// </summary>
public sealed record UpdateDrugProblemRequest(
    Guid? DrugFormId,
    string? CustomDrugName,
    string? CustomActiveSubstances,
    string? CustomFormulationName,
    string? CustomDosage,
    string? ProblemNote);

/// <summary>
/// Command for updating a drug problem.
/// </summary>
public sealed record UpdateDrugProblemCommand(
    Guid UserId,
    Guid Id,
    UpdateDrugProblemRequest Request) : IRequest<ErrorOr<UpdateDrugProblemResponse>>;

/// <summary>
/// Response payload for updating a drug problem.
/// </summary>
public sealed record UpdateDrugProblemResponse(DrugProblemDetailsDto Item);

/// <summary>
/// Controller for updating drug problems.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Drug Problems")]
[Route("api/drug-problems")]
public sealed class UpdateDrugProblemController(
    IMediator _mediator,
    IValidator<UpdateDrugProblemRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating a drug problem.
    /// </summary>
    public const string RouteName = "UpdateDrugProblem";

    /// <summary>
    /// Partially updates a drug problem for the current user.
    /// </summary>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdateDrugProblemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdateDrugProblemResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDrugProblemRequest request,
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

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        UpdateDrugProblemCommand command = request.MapToCommand(userId, id);
        ErrorOr<UpdateDrugProblemResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UpdateDrugProblemResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
