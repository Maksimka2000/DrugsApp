namespace DrugsApp.Features.DrugInteractionToolFeature.ExecuteRegisteredDrugInteractionToolReport;

/// <summary>
/// Mapping helpers for the registered drug interaction tool report use case [description].
/// </summary>
public static class ExecuteRegisteredDrugInteractionToolReportMapping
{
  /// <summary>
  /// Builds the command for executing a registered drug interaction tool report.
  /// </summary>
  /// <param name="request">Incoming request payload with manual selections and flags.</param>
  /// <param name="userId">Authenticated user identifier.</param>
  /// <returns>Command containing user context and request.</returns>
  public static ExecuteRegisteredDrugInteractionToolReportCommand MapToCommand(
    this ExecuteRegisteredDrugInteractionToolReportRequest request,
    Guid userId) =>
    new(UserId: userId, Request: request);
}
