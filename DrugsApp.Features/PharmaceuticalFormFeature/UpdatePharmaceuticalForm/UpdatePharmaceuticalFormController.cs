using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.PharmaceuticalFormFeature.UpdatePharmaceuticalForm;

/// <summary>
/// Request model for updating pharmaceutical form fields.
/// </summary>
public sealed record UpdatePharmaceuticalFormRequest(
    string? FormulationName);

/// <summary>
/// Response containing updated pharmaceutical form fields.
/// </summary>
public sealed record UpdatedPharmaceuticalFormResponse(
    Guid Id,
    string FormulationName);

/// <summary>
/// Command for updating a pharmaceutical form's formulation name.
/// </summary>
public sealed record UpdatePharmaceuticalFormCommand(
    Guid PharmaceuticalFormId,
    UpdatePharmaceuticalFormRequest Request) : IRequest<ErrorOr<UpdatedPharmaceuticalFormResponse>>;

/// <summary>
/// Controller for updating pharmaceutical form information.
/// </summary>
[ApiController]
[Tags("Pharmaceutical Forms")]
[Route("api/pharmaceutical-forms")]
public sealed class UpdatePharmaceuticalFormController : ControllerBase
{
    public const string RouteName = "UpdatePharmaceuticalForm";

    private readonly IMediator _mediator;
    private readonly IValidator<UpdatePharmaceuticalFormRequest> _validator;

    public UpdatePharmaceuticalFormController(
        IMediator mediator,
        IValidator<UpdatePharmaceuticalFormRequest> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    /// <summary>
    /// Updates the formulation name of a pharmaceutical form.
    /// </summary>
    /// <param name="id">Pharmaceutical form unique identifier.</param>
    /// <param name="request">Update request containing new formulation name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated pharmaceutical form information.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Pharmaceutical form not found.</response>
    /// <response code="409">A pharmaceutical form with the same name already exists.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedPharmaceuticalFormResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedPharmaceuticalFormResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdatePharmaceuticalFormRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdatePharmaceuticalFormCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedPharmaceuticalFormResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext)
        );
    }
}


