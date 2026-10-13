using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionReport;

/// <summary>
/// Validates the selection lists of the provider safety report request (up to 20 items of each kind).
/// </summary>
public sealed class ProviderPatientInteractionReportRequestValidator
    : AbstractValidator<ProviderPatientInteractionReportRequest>
{
    public ProviderPatientInteractionReportRequestValidator()
    {
        this.AddIdSelectionRules(request => request.ManualDrugFormIds, "ManualDrugFormIds", 20);
        this.AddIdSelectionRules(request => request.SelectedSupplementIds, "SelectedSupplementIds", 20);
        this.AddIdSelectionRules(request => request.SelectedFoodIds, "SelectedFoodIds", 20);
    }
}
