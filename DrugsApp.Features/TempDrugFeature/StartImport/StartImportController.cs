using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.StartImport;

/// <summary>
/// Request model for starting import execution for a batch.
/// </summary>
public sealed record StartImportRequest(
    Dictionary<string, string> MappingPlan,
    string? Notes);

/// <summary>
/// Response returned when import execution is accepted.
/// </summary>
public sealed record StartImportResponse(
    Guid BatchId,
    string Status);

/// <summary>
/// Command for starting import execution.
/// </summary>
public sealed record StartImportCommand(
    Guid BatchId,
    StartImportRequest Request) : IRequest<ErrorOr<StartImportResponse>>;

/// <summary>
/// Controller for starting import execution.
/// </summary>
[ApiController]
[Tags("TempDrugImport")]
[Route("api/temp-drug-imports")]
public sealed class StartImportController(
    IMediator _mediator,
    IValidator<StartImportRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Starts import for a created batch and enqueues background processing.
    /// </summary>
    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<StartImportResponse>> Start(
        [FromRoute] Guid id,
        [FromBody] StartImportRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        StartImportCommand command = request.MapToCommand(id);
        ErrorOr<StartImportResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            response => Accepted(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

