using DrugsApp.Features.DrugInteractionToolFeature.ApplicationServices;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugInteractionToolFeature.ExecuteRegisteredDrugInteractionToolReport;

/// <summary>
/// Handles registered drug interaction tool report computation [description].
/// </summary>
public sealed class ExecuteRegisteredDrugInteractionToolReportHandler(
  IDrugInteractionToolReportApplicationService reportApplicationService)
  : IRequestHandler<ExecuteRegisteredDrugInteractionToolReportQuery, ErrorOr<ExecuteRegisteredDrugInteractionToolReportResponse>>
{
  /// <summary>
  /// Executes the registered drug interaction tool report with user context and options.
  /// </summary>
  /// <param name="query">Request including user id, manual selections, and display flags.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Computed registered report or error collection.</returns>
  public async Task<ErrorOr<ExecuteRegisteredDrugInteractionToolReportResponse>> Handle(
    ExecuteRegisteredDrugInteractionToolReportQuery query,
    CancellationToken cancellationToken)
  {
    ErrorOr<RegisteredDrugInteractionToolReportReadModel> reportResult =
      await reportApplicationService.ComputeRegisteredReportAsync(
        query.UserId,
        query.Request.ManualDrugFormIds ?? Array.Empty<Guid>(),
        query.Request.IncludeUserData,
        query.Request.ShowAllDrugsInteractions,
        query.Request.ShowOralGuidance,
        query.Request.ShowAllFoodAndSupplementInteractions,
        cancellationToken,
        selectedSupplementIds: query.Request.SelectedSupplementIds,
        selectedFoodIds: query.Request.SelectedFoodIds,
        showPossibleInteractions: query.Request.ShowPossibleInteractions);

    if (reportResult.IsError)
    {
      return reportResult.Errors;
    }

    return new ExecuteRegisteredDrugInteractionToolReportResponse(Report: reportResult.Value);
  }
}
