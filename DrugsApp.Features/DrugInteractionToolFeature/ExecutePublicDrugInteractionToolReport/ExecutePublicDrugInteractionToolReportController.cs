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
/// Request payload for the public drug interaction tool report.
/// </summary>
/// <param name="ManualDrugFormIds">Manual drug form identifiers provided by the caller.</param>
/// <param name="ShowAllDrugsInteractions">Flag to include all drug interactions.</param>
/// <param name="ShowOralGuidance">Flag to include oral guidance.</param>
/// <param name="ShowAllFoodAndSupplementInteractions">Flag to include all food and supplement interactions.</param>
/// <param name="SelectedSupplementIds">Catalog supplements selected for the analysis (up to 5).</param>
/// <param name="SelectedFoodIds">Catalog foods selected for the analysis (up to 5).</param>
/// <param name="ShowPossibleInteractions">
/// Whether catalog foods and supplements that are not selected are analysed ("possible" interactions); defaults to true.
/// </param>
public sealed record ExecutePublicDrugInteractionToolReportRequest(
  IReadOnlyCollection<Guid>? ManualDrugFormIds,
  bool ShowAllDrugsInteractions,
  bool ShowOralGuidance,
  bool ShowAllFoodAndSupplementInteractions,
  IReadOnlyCollection<Guid>? SelectedSupplementIds = null,
  IReadOnlyCollection<Guid>? SelectedFoodIds = null,
  bool ShowPossibleInteractions = true);

/// <summary>
/// Response envelope for the public drug interaction tool report.
/// </summary>
/// <param name="Report">Computed public report data.</param>
public sealed record ExecutePublicDrugInteractionToolReportResponse(
  PublicDrugInteractionToolReportReadModel Report);

/// <summary>
/// MediatR query for the public drug interaction tool report.
/// </summary>
/// <param name="Request">Request payload for execution.</param>
public sealed record ExecutePublicDrugInteractionToolReportQuery(
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
  /// <param name="request">Request payload with manual selections and flags.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
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

    ExecutePublicDrugInteractionToolReportQuery query = request.MapToQuery();
    ErrorOr<ExecutePublicDrugInteractionToolReportResponse> result = await mediator.Send(query, cancellationToken);

    return result.Match(
      response => Ok(response),
      errors => errors.MapToProblem(HttpContext));
  }
}
