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
  : IRequestHandler<ExecutePublicDrugInteractionToolReportCommand, ErrorOr<ExecutePublicDrugInteractionToolReportResponse>>
{
  /// <summary>
  /// Executes the public drug interaction tool report using manual selections and flags.
  /// </summary>
  /// <param name="command">Request wrapper carrying manual forms and display options.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Computed report or error collection.</returns>
  public async Task<ErrorOr<ExecutePublicDrugInteractionToolReportResponse>> Handle(
    ExecutePublicDrugInteractionToolReportCommand command,
    CancellationToken cancellationToken)
  {
    ErrorOr<PublicDrugInteractionToolReportReadModel> reportResult =
      await reportApplicationService.ComputePublicReportAsync(
        command.Request.ManualDrugFormIds ?? Array.Empty<Guid>(),
        command.Request.ShowAllDrugsInteractions,
        command.Request.ShowOralGuidance,
        command.Request.ShowAllFoodAndSupplementInteractions,
        cancellationToken);

    if (reportResult.IsError)
    {
      return reportResult.Errors;
    }

    return new ExecutePublicDrugInteractionToolReportResponse(Report: reportResult.Value);
  }
}
