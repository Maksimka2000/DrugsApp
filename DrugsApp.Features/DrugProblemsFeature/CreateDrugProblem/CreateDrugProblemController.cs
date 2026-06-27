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

namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;

/// <summary>
/// Request model for creating a drug problem.
/// </summary>
/// <param name="DrugFormId">Catalog drug form identifier when creating a database-backed entry.</param>
/// <param name="CustomDrugName">Custom drug name when creating a custom entry.</param>
/// <param name="CustomActiveSubstances">Custom active substances text.</param>
/// <param name="CustomFormulationName">Custom formulation text.</param>
/// <param name="CustomDosage">Custom dosage text.</param>
/// <param name="ProblemNote">Problem note describing the issue.</param>
public sealed record CreateDrugProblemRequest(
    Guid? DrugFormId,
    string? CustomDrugName,
    string? CustomActiveSubstances,
    string? CustomFormulationName,
    string? CustomDosage,
    string ProblemNote);

/// <summary>
/// Command for creating a drug problem.
/// </summary>
/// <param name="UserId">Identifier of the current user.</param>
/// <param name="Request">Creation payload.</param>
public sealed record CreateDrugProblemCommand(
    Guid UserId,
    CreateDrugProblemRequest Request) : IRequest<ErrorOr<CreateDrugProblemResponse>>;

/// <summary>
/// Response payload for drug problem creation.
/// </summary>
/// <param name="Item">Created drug problem details.</param>
public sealed record CreateDrugProblemResponse(DrugProblemDetailsDto Item);

/// <summary>
/// Controller for creating drug problems.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Drug Problems")]
[Route("api/drug-problems")]
public sealed class CreateDrugProblemController(
    IMediator _mediator,
    IValidator<CreateDrugProblemRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating a drug problem.
    /// </summary>
    public const string RouteName = "CreateDrugProblem";

    /// <summary>
    /// Creates a new drug problem entry for the current user.
    /// </summary>
    /// <param name="request">Creation payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreateDrugProblemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateDrugProblemResponse>> Create(
        [FromBody] CreateDrugProblemRequest request,
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

        CreateDrugProblemCommand command = request.MapToCommand(userId);
        ErrorOr<CreateDrugProblemResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CreateDrugProblemResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
