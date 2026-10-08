using DrugsApp.Features.DrugInteractionToolFeature.ApplicationServices;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport;

/// <summary>
/// Handles public drug interaction tool report computation [description].
/// </summary>
public sealed class ExecutePublicDrugInteractionToolReportHandler(
  IDrugInteractionToolReportApplicationService reportApplicationService)
  : IRequestHandler<ExecutePublicDrugInteractionToolReportQuery, ErrorOr<ExecutePublicDrugInteractionToolReportResponse>>
{
  /// <summary>
  /// Executes the public drug interaction tool report using manual selections and flags.
  /// </summary>
  /// <param name="query">Request wrapper carrying manual forms and display options.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Computed report or error collection.</returns>
  public async Task<ErrorOr<ExecutePublicDrugInteractionToolReportResponse>> Handle(
    ExecutePublicDrugInteractionToolReportQuery query,
    CancellationToken cancellationToken)
  {
    ErrorOr<PublicDrugInteractionToolReportReadModel> reportResult =
      await reportApplicationService.ComputePublicReportAsync(
        query.Request.ManualDrugFormIds ?? Array.Empty<Guid>(),
        query.Request.ShowAllDrugsInteractions,
        query.Request.ShowOralGuidance,
        query.Request.ShowAllFoodAndSupplementInteractions,
        cancellationToken);

    if (reportResult.IsError)
    {
      return reportResult.Errors;
    }

    return new ExecutePublicDrugInteractionToolReportResponse(Report: reportResult.Value);
  }
}
