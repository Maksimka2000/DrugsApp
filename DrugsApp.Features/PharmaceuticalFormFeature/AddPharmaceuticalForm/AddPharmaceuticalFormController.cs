using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PharmaceuticalFormFeature.UpdatePharmaceuticalForm;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.PharmaceuticalFormFeature.AddPharmaceuticalForm;

/// <summary>
/// Request model for adding a pharmaceutical form.
/// </summary>
public sealed record AddPharmaceuticalFormRequest(
    string FormulationName);

/// <summary>
/// Response containing created pharmaceutical form details.
/// </summary>
public sealed record CreatedPharmaceuticalFormResponse(
    Guid Id,
    string FormulationName);

/// <summary>
/// Command for adding a new pharmaceutical form.
/// </summary>
public sealed record AddPharmaceuticalFormCommand(
    AddPharmaceuticalFormRequest Request) : IRequest<ErrorOr<CreatedPharmaceuticalFormResponse>>;

/// <summary>
/// Controller for adding pharmaceutical forms.
/// </summary>
[ApiController]
[Tags("Pharmaceutical Forms")]
[Route("api/pharmaceutical-forms")]
public sealed class AddPharmaceuticalFormController(
    IMediator mediator,
    IValidator<AddPharmaceuticalFormRequest> validator) : ControllerBase
{
    public const string RouteName = "AddPharmaceuticalForm";

    private readonly IMediator _mediator = mediator;
    private readonly IValidator<AddPharmaceuticalFormRequest> _validator = validator;

    /// <summary>
    /// Creates a new pharmaceutical form.
    /// </summary>
    /// <param name="request">Create request containing formulation name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns the created pharmaceutical form.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="409">A pharmaceutical form with the same name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreatedPharmaceuticalFormResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedPharmaceuticalFormResponse>> Create(
        [FromBody] AddPharmaceuticalFormRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddPharmaceuticalFormCommand command = request.MapToCommand();
        ErrorOr<CreatedPharmaceuticalFormResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            created => CreatedAtRoute(
                routeName: UpdatePharmaceuticalFormController.RouteName,
                routeValues: new { id = created.Id },
                value: created),
            errors => errors.MapToProblem(HttpContext));
    }
}


