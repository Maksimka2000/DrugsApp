using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugFeature.UpdateDrugForm;

/// <summary>
/// Request model for partially updating a drug form.
/// </summary>
public sealed record UpdateDrugFormRequest(
    string? AtcCode,
    bool? ClearAtcCode,
    string? Dosage,
    bool? ClearDosage,
    string? Leaflet,
    bool? ClearLeaflet,
    string? EducationMaterials,
    bool? ClearEducationMaterials,
    List<Guid>? RouteOfAdministrationIds,
    List<Guid>? PharmaceuticalFormIds);

/// <summary>
/// Response containing updated drug form information.
/// </summary>
public sealed record UpdatedDrugFormResponse(
    Guid DrugId,
    Guid DrugFormId,
    int ProductIdentifier,
    string? AtcCode,
    string? Dosage,
    string? Leaflet,
    string? EducationMaterials,
    bool IsOral,
    List<LookupDto> RoutesOfAdministration,
    List<LookupDto> PharmaceuticalForms);

/// <summary>
/// Command for partially updating a drug form.
/// </summary>
public sealed record UpdateDrugFormCommand(
    Guid DrugId,
    Guid DrugFormId,
    UpdateDrugFormRequest Request) : IRequest<ErrorOr<UpdatedDrugFormResponse>>;

/// <summary>
/// Controller for updating drug form information.
/// </summary>
[ApiController]
[Tags("Drugs")]
[Route("api/drugs")]
public sealed class UpdateDrugFormController(
    IMediator _mediator,
    IValidator<UpdateDrugFormRequest> _validator) : ControllerBase
{
    public const string RouteName = "UpdateDrugForm";

    /// <summary>
    /// Partially updates a drug form belonging to the specified drug.
    /// </summary>
    /// <param name="drugId">Owning drug identifier.</param>
    /// <param name="drugFormId">Drug form identifier.</param>
    /// <param name="request">PATCH payload with fields to change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated drug form information.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Drug form not found or does not belong to the drug.</response>
    [HttpPatch("{drugId:guid}/drug-forms/{drugFormId:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedDrugFormResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdatedDrugFormResponse>> Update(
        [FromRoute] Guid drugId,
        [FromRoute] Guid drugFormId,
        [FromBody] UpdateDrugFormRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateDrugFormCommand command = request.MapToCommand(drugId, drugFormId);
        ErrorOr<UpdatedDrugFormResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

