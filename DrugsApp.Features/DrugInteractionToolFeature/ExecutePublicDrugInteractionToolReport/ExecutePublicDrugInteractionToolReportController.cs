using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport;

/// <summary>
/// Request payload for the public drug interaction tool report [description].
/// </summary>
public sealed record ExecutePublicDrugInteractionToolReportRequest(
  IReadOnlyCollection<Guid>? ManualDrugFormIds,
  bool ShowAllDrugsInteractions,
  bool ShowOralGuidance,
  bool ShowAllFoodAndSupplementInteractions);

/// <summary>
/// Response envelope for the public drug interaction tool report [description].
/// </summary>
public sealed record ExecutePublicDrugInteractionToolReportResponse(
  PublicDrugInteractionToolReportReadModel Report);

/// <summary>
/// MediatR command for the public drug interaction tool report [description].
/// </summary>
public sealed record ExecutePublicDrugInteractionToolReportCommand(
  ExecutePublicDrugInteractionToolReportRequest Request) : IRequest<ErrorOr<ExecutePublicDrugInteractionToolReportResponse>>;

[ApiController]
[AllowAnonymous]
[Tags("Drug Interaction Tool")]
[Route("api/tools/drug-interactions/public")]
public sealed class ExecutePublicDrugInteractionToolReportController(
  IMediator mediator,
  IValidator<ExecutePublicDrugInteractionToolReportRequest> validator) : ControllerBase
{
  public const string RouteName = "ExecutePublicDrugInteractionToolReport";

  /// <summary>
  /// Computes the anonymous public drug interaction tool report (CASE TWO).
  /// </summary>
  [HttpPost("report", Name = RouteName)]
  [ProducesResponseType(typeof(ExecutePublicDrugInteractionToolReportResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ExecutePublicDrugInteractionToolReportResponse>> Execute(
    [FromBody] ExecutePublicDrugInteractionToolReportRequest request,
    CancellationToken cancellationToken)
  {
    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
    {
      return validationResult.MapToProblem(HttpContext);
    }

    ExecutePublicDrugInteractionToolReportCommand command = request.MapToCommand();
    ErrorOr<ExecutePublicDrugInteractionToolReportResponse> result = await mediator.Send(command, cancellationToken);

    return result.Match(
      response => Ok(response),
      errors => errors.MapToProblem(HttpContext));
  }
}
