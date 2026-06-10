using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblemType;

/// <summary>
/// Request model for creating a drug problem type.
/// </summary>
public sealed record CreateDrugProblemTypeRequest(string ProblemTypeName);

/// <summary>
/// Command for creating a drug problem type.
/// </summary>
public sealed record CreateDrugProblemTypeCommand(CreateDrugProblemTypeRequest Request) : IRequest<ErrorOr<DrugProblemTypeDto>>;

/// <summary>
/// Controller for creating drug problem types.
/// </summary>
[ApiController]
[Authorize(Policy = "ManageOther")]
[Tags("Drug Problem Types")]
[Route("api/drug-problem-types")]
public sealed class CreateDrugProblemTypeController(
    IMediator _mediator,
    IValidator<CreateDrugProblemTypeRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating a drug problem type.
    /// </summary>
    public const string RouteName = "CreateDrugProblemType";

    /// <summary>
    /// Creates a new drug problem type entry.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(DrugProblemTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DrugProblemTypeDto>> Create(
        [FromBody] CreateDrugProblemTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateDrugProblemTypeCommand command = request.MapToCommand();
        ErrorOr<DrugProblemTypeDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<DrugProblemTypeDto>>(
            response => CreatedAtRoute(
                routeName: UpdateDrugProblemType.UpdateDrugProblemTypeController.RouteName,
                routeValues: new { id = response.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
