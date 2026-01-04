using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.CreateDrugFormFromTempDrugReview;

/// <summary>
/// Request model for creating a drug form from a temporary drug review.
/// </summary>
public sealed record CreateDrugFormFromTempDrugReviewRequest(
    string? Note,
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    Guid[] ActiveSubstanceIds,
    string ProductIdentifier,
    string? AtcCode,
    string? Dosage,
    string? Leaflet,
    string? EducationalMaterials,
    Guid[]? RouteOfAdministrationIds,
    Guid[]? PharmaceuticalFormIds);

/// <summary>
/// Response model for creating a drug form from a temporary drug review.
/// </summary>
public sealed record CreatedDrugFormFromTempDrugReviewResponse(
    Guid TempDrugId,
    TempDrugStatus NewStatus,
    Guid LinkedDrugFormId,
    string? Note,
    DrugFormDto DrugForm);

/// <summary>
/// Command for creating a drug form from a temporary drug review.
/// </summary>
public sealed record CreateDrugFormFromTempDrugReviewCommand(
    Guid TempDrugId,
    CreateDrugFormFromTempDrugReviewRequest Request)
    : IRequest<ErrorOr<CreatedDrugFormFromTempDrugReviewResponse>>;

/// <summary>
/// Controller for creating a drug form from a temporary drug review.
/// </summary>
[ApiController]
[Tags("Temp Drugs")]
[Route("api/temp-drugs")]
public sealed class CreateDrugFormFromTempDrugReviewController(
    IMediator _mediator,
    IValidator<CreateDrugFormFromTempDrugReviewRequest> _validator) : ControllerBase
{   
    /// <summary>
    /// Creates a drug form from a temporary drug review.
    /// </summary>
    /// <param name="id">The id of the temporary drug.</param>
    /// <param name="request">The request to create a drug form from a temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">The created drug form from a temporary drug review.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="404">The temporary drug not found.</response>
    /// <returns>The created drug form from a temporary drug review.</returns>
    [HttpPost("{id:guid}/drug-forms")]
    [ProducesResponseType(typeof(CreatedDrugFormFromTempDrugReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatedDrugFormFromTempDrugReviewResponse>> Create(
        [FromRoute] Guid id,
        [FromBody] CreateDrugFormFromTempDrugReviewRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateDrugFormFromTempDrugReviewCommand command = request.MapToCommand(id);
        ErrorOr<CreatedDrugFormFromTempDrugReviewResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

