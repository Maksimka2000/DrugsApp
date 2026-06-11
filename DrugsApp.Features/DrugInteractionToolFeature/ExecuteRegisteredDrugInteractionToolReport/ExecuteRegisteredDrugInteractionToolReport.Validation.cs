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
    When(
      request => request.ManualDrugFormIds is not null && request.ManualDrugFormIds.Count > 0,
      () =>
      {
        RuleFor(request => request.ManualDrugFormIds!)
          .Must(ids => ids.Count <= 20)
          .WithMessage("ManualDrugFormIds must not contain more than 20 items.")
          .Must(ids => ids.All(id => id != Guid.Empty))
          .WithMessage("ManualDrugFormIds must not contain empty Guid values.")
          .Must(ids => ids.Distinct().Count() == ids.Count)
          .WithMessage("ManualDrugFormIds must be unique.");
      });
  }
}
