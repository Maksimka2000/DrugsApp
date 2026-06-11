using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugInteractionToolFeature.ExecuteRegisteredDrugInteractionToolReport;

/// <summary>
/// Request payload for the registered drug interaction tool report [description].
/// </summary>
public sealed record ExecuteRegisteredDrugInteractionToolReportRequest(
  IReadOnlyCollection<Guid>? ManualDrugFormIds,
  bool IncludeUserData,
  bool ShowAllDrugsInteractions,
  bool ShowOralGuidance,
  bool ShowAllFoodAndSupplementInteractions);

/// <summary>
/// Response envelope for the registered drug interaction tool report [description].
/// </summary>
public sealed record ExecuteRegisteredDrugInteractionToolReportResponse(
  RegisteredDrugInteractionToolReportReadModel Report);

/// <summary>
/// MediatR command for the registered drug interaction tool report [description].
/// </summary>
public sealed record ExecuteRegisteredDrugInteractionToolReportCommand(
  Guid UserId,
  ExecuteRegisteredDrugInteractionToolReportRequest Request)
  : IRequest<ErrorOr<ExecuteRegisteredDrugInteractionToolReportResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Drug Interaction Tool")]
[Route("api/tools/drug-interactions/registered")]
public sealed class ExecuteRegisteredDrugInteractionToolReportController(
  IMediator mediator,
  IValidator<ExecuteRegisteredDrugInteractionToolReportRequest> validator) : ControllerBase
{
  public const string RouteName = "ExecuteRegisteredDrugInteractionToolReport";

  /// <summary>
  /// Computes the authenticated client drug interaction tool report (CASE ONE).
  /// </summary>
  [HttpPost("report", Name = RouteName)]
  [ProducesResponseType(typeof(ExecuteRegisteredDrugInteractionToolReportResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<ActionResult<ExecuteRegisteredDrugInteractionToolReportResponse>> Execute(
    [FromBody] ExecuteRegisteredDrugInteractionToolReportRequest request,
    CancellationToken cancellationToken)
  {
    ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
    {
      return validationResult.MapToProblem(HttpContext);
    }

    string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(subject, out Guid userId))
    {
      return Unauthorized();
    }

    ExecuteRegisteredDrugInteractionToolReportCommand command = request.MapToCommand(userId);
    ErrorOr<ExecuteRegisteredDrugInteractionToolReportResponse> result =
      await mediator.Send(command, cancellationToken);

    return result.Match(
      response => Ok(response),
      errors => errors.MapToProblem(HttpContext));
  }
}
