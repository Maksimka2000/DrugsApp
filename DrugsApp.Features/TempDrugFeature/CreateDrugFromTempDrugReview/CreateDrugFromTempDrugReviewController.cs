using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview;

/// <summary>
/// Request model for creating a drug from a temporary drug review.
/// </summary>
public sealed record CreateDrugFromTempDrugReviewRequest(
    string? Note,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    Guid[] ActiveSubstanceIds,
    string ProductIdentifier,
    string? AtcCode,
    string? Dosage,
    string? Leaflet,
    string? EducationalMaterials,
    Guid[]? RouteOfAdministrationIds,
    Guid[]? PharmaceuticalFormIds);

/// <summary>
/// Response model for creating a drug from a temporary drug review.
/// </summary>
public sealed record CreatedDrugFromTempDrugReviewResponse(
    Guid TempDrugId,
    TempDrugStatus NewStatus,
    Guid LinkedDrugFormId,
    string? Note,
    DrugDto Drug);

/// <summary>
/// Command for creating a drug from a temporary drug review.
/// </summary>
public sealed record CreateDrugFromTempDrugReviewCommand(
    Guid TempDrugId,
    CreateDrugFromTempDrugReviewRequest Request)
    : IRequest<ErrorOr<CreatedDrugFromTempDrugReviewResponse>>;

[ApiController]
[Tags("Temp Drugs")]
[Route("api/temp-drugs")]
public sealed class CreateDrugFromTempDrugReviewController(
    IMediator _mediator,
    IValidator<CreateDrugFromTempDrugReviewRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Creates a drug from a temporary drug review.
    /// </summary>
    /// <param name="id">The id of the temporary drug.</param>
    /// <param name="request">The request to create a drug from a temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">The created drug from a temporary drug review.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="404">The temporary drug not found.</response>
    [HttpPost("{id:guid}/drugs")]
    [ProducesResponseType(typeof(CreatedDrugFromTempDrugReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatedDrugFromTempDrugReviewResponse>> Create(
        [FromRoute] Guid id,
        [FromBody] CreateDrugFromTempDrugReviewRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateDrugFromTempDrugReviewCommand command = request.MapToCommand(id);
        ErrorOr<CreatedDrugFromTempDrugReviewResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

