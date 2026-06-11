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
