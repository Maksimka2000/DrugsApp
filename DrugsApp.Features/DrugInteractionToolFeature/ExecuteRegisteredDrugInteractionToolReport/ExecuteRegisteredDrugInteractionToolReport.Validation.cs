using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.DrugInteractionToolFeature.ExecuteRegisteredDrugInteractionToolReport;

/// <summary>
/// Validates the registered drug interaction tool report request shape [description].
/// </summary>
public sealed class ExecuteRegisteredDrugInteractionToolReportRequestValidator
  : AbstractValidator<ExecuteRegisteredDrugInteractionToolReportRequest>
{
  public ExecuteRegisteredDrugInteractionToolReportRequestValidator()
  {
    this.AddIdSelectionRules(request => request.ManualDrugFormIds, "ManualDrugFormIds", 20);
    this.AddIdSelectionRules(request => request.SelectedSupplementIds, "SelectedSupplementIds", 20);
    this.AddIdSelectionRules(request => request.SelectedFoodIds, "SelectedFoodIds", 20);
  }
}
