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
  : IRequestHandler<ExecuteRegisteredDrugInteractionToolReportCommand, ErrorOr<ExecuteRegisteredDrugInteractionToolReportResponse>>
{
  /// <summary>
  /// Executes the registered drug interaction tool report with user context and options.
  /// </summary>
  /// <param name="command">Request including user id, manual selections, and display flags.</param>
  /// <param name="cancellationToken">Token to cancel the operation.</param>
  /// <returns>Computed registered report or error collection.</returns>
  public async Task<ErrorOr<ExecuteRegisteredDrugInteractionToolReportResponse>> Handle(
    ExecuteRegisteredDrugInteractionToolReportCommand command,
    CancellationToken cancellationToken)
  {
    ErrorOr<RegisteredDrugInteractionToolReportReadModel> reportResult =
      await reportApplicationService.ComputeRegisteredReportAsync(
        command.UserId,
        command.Request.ManualDrugFormIds ?? Array.Empty<Guid>(),
        command.Request.IncludeUserData,
        command.Request.ShowAllDrugsInteractions,
        command.Request.ShowOralGuidance,
        command.Request.ShowAllFoodAndSupplementInteractions,
        cancellationToken);

    if (reportResult.IsError)
    {
      return reportResult.Errors;
    }

    return new ExecuteRegisteredDrugInteractionToolReportResponse(Report: reportResult.Value);
  }
}
