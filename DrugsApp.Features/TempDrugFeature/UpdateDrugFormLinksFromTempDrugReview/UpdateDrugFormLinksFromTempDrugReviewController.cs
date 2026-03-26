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

namespace DrugsApp.Features.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview;


/// <summary>
/// Request model for updating drug form links from a temporary drug review.
/// </summary>
public sealed record UpdateDrugFormLinksFromTempDrugReviewRequest(
    string? Note,
    Guid DrugId,
    Guid DrugFormId,
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
/// Response model for updating drug form links from a temporary drug review.
/// </summary>
public sealed record UpdatedDrugFormLinksFromTempDrugReviewResponse(
    Guid TempDrugId,
    TempDrugStatus NewStatus,
    Guid LinkedDrugFormId,
    string? Note,
    DrugFormDto DrugForm);

/// <summary>
/// Command for updating drug form links from a temporary drug review.
/// </summary>
public sealed record UpdateDrugFormLinksFromTempDrugReviewCommand(
    Guid TempDrugId,
    UpdateDrugFormLinksFromTempDrugReviewRequest Request)
    : IRequest<ErrorOr<UpdatedDrugFormLinksFromTempDrugReviewResponse>>;


/// <summary>
/// Controller for updating drug form links from a temporary drug review.
/// </summary>
/// <param name="_mediator">The mediator.</param>
/// <param name="_validator">The validator.</param>
[ApiController]
[Tags("Temp Drugs")]
[Route("api/temp-drugs")]
public sealed class UpdateDrugFormLinksFromTempDrugReviewController(
    IMediator _mediator,
    IValidator<UpdateDrugFormLinksFromTempDrugReviewRequest> _validator) : ControllerBase
{

    /// <summary>
    /// Updates the drug form links from a temporary drug review.
    /// </summary>
    /// <param name="id">The id of the temporary drug.</param>
    /// <param name="request">The request to update the drug form links from a temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">The updated drug form links from a temporary drug review.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="404">The temporary drug not found.</response>
    /// <returns>The updated drug form links from a temporary drug review.</returns>
    [HttpPost("{id:guid}/drug-form-link-updates")]
    [ProducesResponseType(typeof(UpdatedDrugFormLinksFromTempDrugReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdatedDrugFormLinksFromTempDrugReviewResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDrugFormLinksFromTempDrugReviewRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateDrugFormLinksFromTempDrugReviewCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedDrugFormLinksFromTempDrugReviewResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

