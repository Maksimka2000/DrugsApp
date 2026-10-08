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
/// Request payload for the registered drug interaction tool report.
/// </summary>
/// <param name="ManualDrugFormIds">Manual drug form identifiers provided by the caller.</param>
/// <param name="IncludeUserData">Flag to merge user data (regular intakes, allergies, problems).</param>
/// <param name="ShowAllDrugsInteractions">Flag to include all drug interactions.</param>
/// <param name="ShowOralGuidance">Flag to include oral guidance.</param>
/// <param name="ShowAllFoodAndSupplementInteractions">Flag to include all food and supplement interactions.</param>
public sealed record ExecuteRegisteredDrugInteractionToolReportRequest(
  IReadOnlyCollection<Guid>? ManualDrugFormIds,
  bool IncludeUserData,
  bool ShowAllDrugsInteractions,
  bool ShowOralGuidance,
  bool ShowAllFoodAndSupplementInteractions);

/// <summary>
/// Response envelope for the registered drug interaction tool report.
/// </summary>
/// <param name="Report">Computed registered report data.</param>
public sealed record ExecuteRegisteredDrugInteractionToolReportResponse(
  RegisteredDrugInteractionToolReportReadModel Report);

/// <summary>
/// MediatR query for the registered drug interaction tool report.
/// </summary>
/// <param name="UserId">Authenticated user identifier.</param>
/// <param name="Request">Request payload for execution.</param>
public sealed record ExecuteRegisteredDrugInteractionToolReportQuery(
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
  /// <param name="request">Request payload with manual selections and display flags.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
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

    ExecuteRegisteredDrugInteractionToolReportQuery query = request.MapToQuery(userId);
    ErrorOr<ExecuteRegisteredDrugInteractionToolReportResponse> result =
      await mediator.Send(query, cancellationToken);

    return result.Match(
      response => Ok(response),
      errors => errors.MapToProblem(HttpContext));
  }
}
