using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.CreateVetDrugFromTempDrugReview;

/// <summary>
/// Request model for creating a veterinary drug from a temporary drug review.
/// </summary>
public sealed record CreateVetDrugFromTempDrugReviewRequest(
    string? Note,
    string CommercialName,
    string TypeOfProduct,
    string ProductIdentifier);

/// <summary>
/// Response model for creating a veterinary drug from a temporary drug review.
/// </summary>
public sealed record CreatedVetDrugFromTempDrugReviewResponse(
    Guid TempDrugId,
    TempDrugStatus NewStatus,
    Guid LinkedVetDrugId,
    string? Note,
    VetDrugDto VetDrug);

/// <summary>
/// Command for creating a veterinary drug from a temporary drug review.
/// </summary>
public sealed record CreateVetDrugFromTempDrugReviewCommand(
    Guid TempDrugId,
    CreateVetDrugFromTempDrugReviewRequest Request)
    : IRequest<ErrorOr<CreatedVetDrugFromTempDrugReviewResponse>>;

/// <summary>
///             
/// </summary>
/// <param name="_mediator"></param>
/// <param name="_validator"></param>
[ApiController]
[Tags("Temp Drugs")]
[Route("api/temp-drugs")]
public sealed class CreateVetDrugFromTempDrugReviewController(
    IMediator _mediator,
    IValidator<CreateVetDrugFromTempDrugReviewRequest> _validator) : ControllerBase
{

    /// <summary>
    /// Creates a veterinary drug from a temporary drug review.
    /// </summary>
    /// <param name="id">The id of the temporary drug.</param>
    /// <param name="request">The request containing the veterinary drug information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="201">The created veterinary drug.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="404">The temporary drug is not found.</response>
    /// <response code="409">The veterinary drug already exists.</response>
    /// <returns>The created veterinary drug.</returns>
    [HttpPost("{id:guid}/vet-drugs")]
    [ProducesResponseType(typeof(CreatedVetDrugFromTempDrugReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedVetDrugFromTempDrugReviewResponse>> Create(
        [FromRoute] Guid id,
        [FromBody] CreateVetDrugFromTempDrugReviewRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateVetDrugFromTempDrugReviewCommand command = request.MapToCommand(id);
        ErrorOr<CreatedVetDrugFromTempDrugReviewResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

