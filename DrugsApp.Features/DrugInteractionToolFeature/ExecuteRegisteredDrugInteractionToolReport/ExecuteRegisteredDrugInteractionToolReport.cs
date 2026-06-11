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
