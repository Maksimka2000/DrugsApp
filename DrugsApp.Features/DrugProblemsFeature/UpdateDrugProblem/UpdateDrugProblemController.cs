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
/// <param name="DrugFormId">Catalog drug form id to switch to.</param>
/// <param name="CustomDrugName">Custom drug name when source is custom.</param>
/// <param name="CustomActiveSubstances">Custom active substances text.</param>
/// <param name="CustomFormulationName">Custom pharmaceutical form text.</param>
/// <param name="CustomDosage">Custom dosage text.</param>
/// <param name="ProblemNote">Optional problem note update.</param>
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
/// <param name="UserId">Identifier of the current user.</param>
/// <param name="Id">Identifier of the drug problem to update.</param>
/// <param name="Request">Update payload.</param>
public sealed record UpdateDrugProblemCommand(
    Guid UserId,
    Guid Id,
    UpdateDrugProblemRequest Request) : IRequest<ErrorOr<UpdateDrugProblemResponse>>;

/// <summary>
/// Response payload for updating a drug problem.
/// </summary>
/// <param name="Item">Updated drug problem details.</param>
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
    /// <param name="id">Identifier of the drug problem to update.</param>
    /// <param name="request">Payload containing fields to update.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
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
